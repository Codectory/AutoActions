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

        readonly object _processesLock = new object();
        readonly object _accessLock = new object();

        public ApplicationItem CurrentRunningApplicationItem { get; private set; }
        public ApplicationItem CurrentFocusedApplicationItem { get; private set; }

        Dictionary<ApplicationItem, ApplicationState> _applications = new Dictionary<ApplicationItem, ApplicationState>();
        public IReadOnlyDictionary<ApplicationItem, ApplicationState> Applications => new ReadOnlyDictionary<ApplicationItem, ApplicationState>(_applications);

        bool _stopRequested = false;
        bool _isRunning = false;
        public bool IsRunning { get => _isRunning; private set => _isRunning = value; }

        public event EventHandler<string> NewLog;
        public event EventHandler OneProcessIsRunningChanged;
        public event EventHandler OneProcessIsFocusedChanged;

        ManagementEventWatcher startWatch;
        ManagementEventWatcher stopWatch;

        public ProcessWatcher()
        {
            startWatch = new ManagementEventWatcher(
      new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            stopWatch = new ManagementEventWatcher(
      new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            startWatch.EventArrived += StartWatch_EventArrived;
            stopWatch.EventArrived += StopWatch_EventArrived;

        }


        private void CallNewLog(string logMessage)
        {
            NewLog?.Invoke(this, logMessage);
        }
  

        private void StartWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            lock (_processesLock)
            {
                string applicationName = e.NewEvent.Properties["ProcessName"].Value.ToString().Replace(".exe", "").ToUpperInvariant();
                if (_applications.Any(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)))
                {
                    ApplicationItem application = _applications.First(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)).Key;
                    if (_applications[application] == ApplicationState.None)
                        _applications[application] = ApplicationState.Running;
                    CallNewLog($"Application startet: {application}");
                }
            }
        }

        private void StopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {

            lock (_processesLock)
            {
                string applicationName = e.NewEvent.Properties["ProcessName"].Value.ToString().Replace(".exe", "").ToUpperInvariant();
                if (_applications.Any(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)))
                {
                    ApplicationItem application = _applications.First(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)).Key;
                    _applications[application] = ApplicationState.None;
                    CallNewLog($"Application stopped:  {application}");
                }
            }
        }

        public void AddProcess(ApplicationItem application)
        {
            lock (_processesLock)
            {
                if (!_applications.ContainsKey(application))
                {
                    _applications.Add(application, ApplicationState.None);
                    CallNewLog($"Application added to process watcher: {application}");
                }
                UpdateRunningProcessesOnce();
            }
        }

        public void RemoveProcess(ApplicationItem application)
        {
            lock (_processesLock)
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
                startWatch.Start();
                stopWatch.Start();
                UpdateRunningProcessesOnce();
                _isRunning = true;
                _watchProcessThread = new Thread(WatchProcessLoop);
                _watchProcessThread.IsBackground = true;
                _watchProcessThread.Start();
                CallNewLog($"rocess watcher started");
            }
        }

        public void Stop()
        {
            if (_stopRequested || !IsRunning)
                return;
            lock (_accessLock)
            {
                CallNewLog($"Stopping process watcher...");
                startWatch.Stop();
                stopWatch.Stop();

                _stopRequested = true;
                _watchProcessThread.Join();
                _stopRequested = false;
                _isRunning = false;
                _watchProcessThread = null;
                CallNewLog($"Process watcher stopped.");
            }
        }


        private void UpdateRunningProcessesOnce()
        {
            lock (_processesLock)
            {
                CallNewLog($"Looking for running applications on start...");

                Process[] processes = Process.GetProcesses();

                List<ApplicationItem> applications = _applications.Select(a => a.Key).ToList();
                foreach (ApplicationItem application in applications)
                {
                    _applications[application] = ApplicationState.None;
                    foreach (var process in processes.Select(p => p.ProcessName))
                    {
                        if (process.ToUpperInvariant() == application.ApplicationName.ToUpperInvariant())
                        {
                            _applications[application] = ApplicationState.Running;
                            CurrentRunningApplicationItem = application;
                            CallNewLog($"Application is running: {application}");

                            return;
                        }
                    }
                }
                CallNewLog($"No application is running.");

                CurrentRunningApplicationItem = null;
            }
        }


        private void WatchProcessLoop()
        {
            while (!_stopRequested)
            {
                lock (_processesLock)
                {
                    bool oldIsOneRunning = OneProcessIsRunning;
                    bool newIsOneRunning = GetIsOneProcessRunning();
                    OneProcessIsRunning = newIsOneRunning;
                    if (oldIsOneRunning != newIsOneRunning)
                    {
                        CallNewLog($"Running processes changed: {CurrentRunningApplicationItem}");
                        OneProcessIsRunningChanged?.Invoke(this, EventArgs.Empty);
                    }
                    
                    bool oldIsOneFocused = OneProcessIsFocused;
                    bool newIsOneFocused = GetIsOneProcessFocused();
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

    


        private bool GetIsOneProcessRunning()
        {
            if (_applications.Any(a => a.Value != ApplicationState.None))
            {
                var application = _applications.First(a => a.Value != ApplicationState.None);
                CurrentRunningApplicationItem = application.Key;
                return true;

            }
            else
            {
                CurrentRunningApplicationItem = null;
                return false;
            }
        }

        private bool GetIsOneProcessFocused()
        {
            string currentProcessName = GetForegroundProcessName().ToUpperInvariant();
            lock (_processesLock)
            {
                if (_applications.Any(a => a.Key.ApplicationName.ToUpperInvariant().Equals(currentProcessName)))
                {
                    var application = _applications.First(a => a.Key.ApplicationName.ToUpperInvariant().Equals(currentProcessName)).Key;
                    _applications[application] = ApplicationState.Focused;
                    CurrentFocusedApplicationItem = application;
                    return true;

                }
                else
                {
                    foreach (var application in _applications.Select(a => a.Key))
                        if (_applications[application] != ApplicationState.None)
                            _applications[application] = ApplicationState.Running;
                    CurrentFocusedApplicationItem = null;
                    return false;
                }

            }
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
