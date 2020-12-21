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



        Dictionary<ApplicationItem, bool> _applications = new Dictionary<ApplicationItem, bool>();
        public IReadOnlyDictionary<ApplicationItem, bool> Applications => new ReadOnlyDictionary<ApplicationItem, bool>(_applications);

        bool _stopRequested = false;
        bool _isRunning = false;
        public bool IsRunning { get => _isRunning; private set => _isRunning = value; }

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

  

        private void StartWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            lock (_processesLock)
            {
                string applicationName = e.NewEvent.Properties["ProcessName"].Value.ToString().Replace(".exe", "").ToUpperInvariant();
                if (_applications.Any(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)))
                {
                    ApplicationItem application = _applications.First(a => a.Key.ApplicationName.ToUpperInvariant().Equals(applicationName)).Key;
                    bool oldValue = _applications[application];
                    _applications[application] = true;
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
                    bool oldValue = _applications[application];
                    _applications[application] = false;
                }
            }
        }

        public void AddProcess(ApplicationItem applicationItem)
        {
            lock (_processesLock)
            {
                if (!_applications.ContainsKey(applicationItem))
                {
                    _applications.Add(applicationItem, false);
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
                }
            }
        }


        public void Start()
        {
            if (_stopRequested || IsRunning)
                return;
            lock (_accessLock)
            {
                UpdateRunningProcessesOnce();
                startWatch.Start();
                stopWatch.Start();
                _isRunning = true;
                _watchProcessThread = new Thread(WatchProcessLoop);
                _watchProcessThread.IsBackground = true;
                _watchProcessThread.Start();
            }
        }

        public void Stop()
        {
            if (_stopRequested || !IsRunning)
                return;
            lock (_accessLock)
            {
                startWatch.Stop();
                stopWatch.Stop();

                _stopRequested = true;
                _watchProcessThread.Join();
                _stopRequested = false;
                _isRunning = false;
                _watchProcessThread = null;
            }
        }


        private void UpdateRunningProcessesOnce()
        {
            lock (_processesLock)
            {
                Process[] processes = Process.GetProcesses();


                List<ApplicationItem> applications = _applications.Select(a => a.Key).ToList();
                foreach (ApplicationItem application in applications)
                {
                    _applications[application] = false;
                    foreach (var process in processes.Select(p => p.ProcessName))
                    {
                        if (process.ToUpperInvariant() == application.ApplicationName.ToUpperInvariant())
                        {
                            _applications[application] = true;
                            break;
                        }
                    }
                }
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
                        OneProcessIsRunningChanged?.Invoke(this, EventArgs.Empty);
                    
                    bool oldIsOneFocused = OneProcessIsFocused;
                    bool newIsOneFocused = GetIsOneProcessFocused();
                    OneProcessIsFocused = newIsOneFocused;
                    if (oldIsOneFocused != newIsOneFocused)
                        OneProcessIsFocusedChanged?.Invoke(this, EventArgs.Empty);
                }
                Thread.Sleep(150);
            }
        }

    


        private bool GetIsOneProcessRunning()
        {
            if (_applications.Any(a => a.Value == true))
                return true;
            else
                return false;
        }

        private bool GetIsOneProcessFocused()
        {
            string currentProcessName = GetForegroundProcessName().ToUpperInvariant();
            bool result = false;
            lock (_processesLock)
            {
                if (_applications.Any(a => a.Key.ApplicationName.ToUpperInvariant().Equals(currentProcessName)))
                    result = true;
            }
            return result;
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
