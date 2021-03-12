using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HDRProfile
{
    public class ProcessWatcher
    {
        public bool OneProcessIsRunning { get; private set; } = false;
        public bool OneProcessIsFocused { get; private set; } = false;

        Thread _watchProcessThread = null;

        readonly object _applicationsLock = new object();
        readonly object _accessLock = new object();

        public ApplicationItem CurrentRunningApplicationItem { get; private set; }
        public ApplicationItem CurrentFocusedApplicationItem { get; private set; }

        Dictionary<ApplicationItem, ApplicationState> _applications = new Dictionary<ApplicationItem, ApplicationState>();
        public IReadOnlyDictionary<ApplicationItem, ApplicationState> Applications
        {
            get
            {
                lock (_applicationsLock)
                    return new ReadOnlyDictionary<ApplicationItem, ApplicationState>(_applications.ToDictionary(entry => entry.Key, entry => entry.Value));
            }
        }

        bool _stopRequested = false;
        bool _isRunning = false;
        public bool IsRunning { get => _isRunning; private set => _isRunning = value; }

        public event EventHandler<string> NewLog;
        public event EventHandler OneProcessIsRunningChanged;
        public event EventHandler OneProcessIsFocusedChanged;

        //ManagementEventWatcher startWatch;
        //ManagementEventWatcher stopWatch;

        public ProcessWatcher()
        {
      //      startWatch = new ManagementEventWatcher(
      //new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
      //      stopWatch = new ManagementEventWatcher(
      //new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
      //      startWatch.EventArrived += StartWatch_EventArrived;
      //      stopWatch.EventArrived += StopWatch_EventArrived;

        }


        private void CallNewLog(string logMessage)
        {
            NewLog?.Invoke(this, logMessage);
        }
  

        //private void StartWatch_EventArrived(object sender, EventArrivedEventArgs e)
        //{
        //    lock (_applicationsLock)
        //    {
        //        string applicationName = e.NewEvent.Properties["ProcessName"].Value.ToString().Replace(".exe", "").ToUpperInvariant();
        //        if (_applications.Any(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)))
        //        {
        //            ApplicationItem application = _applications.First(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)).Key;
        //            if (_applications[application] == ApplicationState.None)
        //                _applications[application] = ApplicationState.Running;
        //            CallNewLog($"Application startet: {application}");
        //        }
        //    }
        //}

        //private void StopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        //{

        //    lock (_applicationsLock)
        //    {
        //        string applicationName = e.NewEvent.Properties["ProcessName"].Value.ToString().Replace(".exe", "").ToUpperInvariant();
        //        if (_applications.Any(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)))
        //        {
        //            ApplicationItem application = _applications.First(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)).Key;
        //            _applications[application] = ApplicationState.None;
        //            CallNewLog($"Application stopped:  {application}");
        //        }
        //    }
        //}

        public void AddProcess(ApplicationItem application)
        {
            lock (_applicationsLock)
            {
                if (!_applications.ContainsKey(application))
                {
                    _applications.Add(application, ApplicationState.None);
                    CallNewLog($"Application added to process watcher: {application}");
                }
            }
        }

        public void RemoveProcess(ApplicationItem application)
        {
            lock (_applicationsLock)
            {
                if (_applications.ContainsKey(application))
                {
                    _applications.Remove(application);
                    CallNewLog($"Application removed process watcher: {application}");
                }
            }
        }


        public void Start()
        {
            if (_stopRequested || IsRunning)
                return;
            lock (_accessLock)
            {
                CallNewLog($"Starting process watcher...");
                //startWatch.Start();
                //stopWatch.Start();
                _isRunning = true;
                _watchProcessThread = new Thread(WatchProcessLoop);
                _watchProcessThread.IsBackground = true;
                _watchProcessThread.Start();
                CallNewLog($"Process watcher started");
            }
        }

        public void Stop()
        {
            if (_stopRequested || !IsRunning)
                return;
            lock (_accessLock)
            {
                CallNewLog($"Stopping process watcher...");
                //startWatch.Stop();
                //stopWatch.Stop();
                _stopRequested = true;
                _watchProcessThread.Join();
                _stopRequested = false;
                _isRunning = false;
                _watchProcessThread = null;
                CallNewLog($"Process watcher stopped.");
            }
        }

        private void WatchProcessLoop()
        {
            while (!_stopRequested)
            {
                lock (_applicationsLock)
                {
                    UpdateApplications();
                    bool oldIsOneRunning = OneProcessIsRunning;
                    bool newIsOneRunning = _applications.Any(a => a.Value == ApplicationState.Running ||a.Value == ApplicationState.Focused);
                    OneProcessIsRunning = newIsOneRunning;
                    if (oldIsOneRunning != newIsOneRunning)
                    {
                        CallNewLog($"Running processes changed: {CurrentRunningApplicationItem}");
                        OneProcessIsRunningChanged?.Invoke(this, EventArgs.Empty);
                    }
                    
                    bool oldIsOneFocused = OneProcessIsFocused;
                    bool newIsOneFocused = _applications.Any(a => a.Value == ApplicationState.Focused);
                    OneProcessIsFocused = newIsOneFocused;
                    if (oldIsOneFocused != newIsOneFocused)
                    {
                        CallNewLog($"Focused process changed: {CurrentFocusedApplicationItem}");
                        OneProcessIsFocusedChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                Thread.Sleep(150);
            }
        }

        private void UpdateApplications()
        {

            lock (_applicationsLock)
            {

                Process[] processes = Process.GetProcesses();

                List<ApplicationItem> applications = _applications.Select(a => a.Key).ToList();
                ApplicationItem newCurrentRunningApplicationItem = null;
                ApplicationItem newCurrentFocusedApplicationItem = null;
                foreach (ApplicationItem application in applications)
                {
                    ApplicationState state = ApplicationState.None;
                    foreach (var process in processes.Select(p => p.ProcessName))
                    {
                        if (process.ToUpperInvariant() == application.ApplicationName.ToUpperInvariant())
                        {
                            state = ApplicationState.Running;
                            newCurrentRunningApplicationItem = application;
                            if (IsFocusedProcess(process))
                            {
                                state = ApplicationState.Focused;
                                newCurrentFocusedApplicationItem = application;
                            }
                        }
                    }
                    _applications[application] = state;
                }
                CurrentRunningApplicationItem = newCurrentRunningApplicationItem;
                CurrentFocusedApplicationItem = CurrentFocusedApplicationItem;
            }
        }



        private bool IsFocusedProcess(string processName)
        {
            string currentProcessName = GetForegroundProcessName().ToUpperInvariant();
            return processName.ToUpperInvariant().Equals(currentProcessName);
        }



        // The GetForegroundWindow function returns a handle to the foreground window
        // (the window  with which the user is currently working).
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // The GetWindowThreadProcessId function retrieves the identifier of the thread
        // that created the specified window and, optionally, the identifier of the
        // process that created the window.
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Returns the name of the process owning the foreground window.
        private string GetForegroundProcessName()
        {
            IntPtr hwnd = GetForegroundWindow();

            // The foreground window can be NULL in certain circumstances, 
            // such as when a window is losing activation.
            if (hwnd == null)
                return "Unknown";

            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.Id == pid)
                    return p.ProcessName.ToUpperInvariant();
            }

            return "Unknown";
        }
    }
}
