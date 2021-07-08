using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoHDR
{
    public class ProcessWatcher
    {
        public bool OneProcessIsRunning { get; private set; } = false;
        public bool OneProcessIsFocused { get; private set; } = false;

        Thread _watchProcessThread = null;

        readonly object _applicationsLock = new object();
        readonly object _accessLock = new object();

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
        //public event EventHandler OneProcessIsRunningChanged;
        //public event EventHandler FocusedProcessChanged;
        public event EventHandler<ApplicationItem> NewRunningApplication;
        public event EventHandler<ApplicationItem> ApplicationClosed;
        public event EventHandler<ApplicationItem> ApplicationGotFocus;
        public event EventHandler<ApplicationItem> ApplicationLostFocus;

        public ProcessWatcher()

        {
        }


        private void CallNewLog(string logMessage)
        {
            NewLog?.Invoke(this, logMessage);
        }
  
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
                    UpdateApplications();
                Thread.Sleep(Tools.GlobalRefreshInterval);
            }
        }

        private void CallNewRunningapplication(ApplicationItem application)
        {
            CallNewLog($"Application started: {application}");

            NewRunningApplication?.Invoke(this, application);
        }
        private void CallApplicationClosed(ApplicationItem application)
        {
            CallNewLog($"Application closed: {application}");
            ApplicationClosed?.Invoke(this, application);
        }

        private void CallApplicationGotFocus(ApplicationItem application)
        {
            CallNewLog($"Application got focus: {application}");
            ApplicationGotFocus?.Invoke(this, application);
        }
        private void CallApplicationLostFocus(ApplicationItem application)
        {
            CallNewLog($"Application lost focus: {application}");
            ApplicationLostFocus?.Invoke(this, application);
        }

        private void UpdateApplications()
        {

            lock (_applicationsLock)
            {


                List<ApplicationItem> applications = _applications.Select(a => a.Key).ToList();

                Process[] processes = Process.GetProcesses();

                foreach (ApplicationItem application in applications)
                {
                    bool callNewRunning = false;
                    bool callGotFocus = false;
                    bool callLostFocus = false;
                    bool callClosed = false;
                    ApplicationState state = ApplicationState.None;
                    ApplicationState oldState = _applications[application];
                    foreach (var process in processes.Select(p => p.ProcessName))
                    {
                        if (process.ToUpperInvariant().Equals(application.ApplicationName.ToUpperInvariant()))
                        {
                            state = ApplicationState.Running;

                            if (oldState == ApplicationState.None)
                                callNewRunning = true;

                            if (IsFocusedApplication(process))
                            {
                                state = ApplicationState.Focused;
                                if (oldState != ApplicationState.Focused)
                                    callGotFocus = true;
                            }
                            else
                            {
                                if (oldState == ApplicationState.Focused)
                                    callLostFocus = true;

                            }
                        }
                    }
                    if (state == ApplicationState.None && oldState != ApplicationState.None)
                        callClosed = true;

                    _applications[application] = state;
                    if (callNewRunning)
                        CallNewRunningapplication(application);
                    if (callGotFocus)
                        CallApplicationGotFocus(application);
                    if (callLostFocus)
                        CallApplicationLostFocus(application);
                    if (callClosed)
                        CallApplicationClosed(application);
                }
            }
        }

        private bool IsFocusedApplication(string processName)
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
