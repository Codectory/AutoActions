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
        public ProcessWatchMode WatchMode { get; set; } = ProcessWatchMode.Focused;

        Thread _watchThread = null;

        readonly object _processesLock = new object();
        readonly object _threadLock = new object();

        ObservableCollection<string> _processNames = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> Processes => new ReadOnlyObservableCollection<string>(_processNames);

        bool _stopRequested = false;
        bool _isRunning = false;
        public bool IsRunning { get => _isRunning; private set => _isRunning = value; }

        public event EventHandler OneProcessIsRunningChanged;
        public event EventHandler OneProcessIsFocusedhanged;


        public void AddProcess(string processFilePath)
        {
            lock (_processesLock)
            {
                if (!_processNames.Contains(processFilePath.ToUpperInvariant()))
                {
                    _processNames.Add(processFilePath.ToUpperInvariant());
                }
            }
        }

        public void Start()
        {
            if (_stopRequested || IsRunning)
                return;
            lock (_threadLock)
            {
                _isRunning = true;
                _watchThread = new Thread(WatchLoop);
                _watchThread.IsBackground = true;
                _watchThread.Start();
            }
        }

        private void WatchLoop()
        {
            while (!_stopRequested)
            {
                if (WatchMode != ProcessWatchMode.None)
                {
                    bool oldValue = WatchMode == ProcessWatchMode.Focused ? OneProcessIsFocused : OneProcessIsRunning;
                    bool newValue = WatchMode == ProcessWatchMode.Focused ? GetIsOneProcessFocused() : GetIsOneProcessRunning();
                    bool changed = oldValue != newValue;
                    if (WatchMode == ProcessWatchMode.Focused)
                    {
                        OneProcessIsFocused = newValue;
                        if (changed)
                            OneProcessIsFocusedhanged?.Invoke(this, EventArgs.Empty);
                    }

                    if (WatchMode == ProcessWatchMode.Running)
                    {
                        OneProcessIsRunning = newValue;
                        if (changed)
                            OneProcessIsRunningChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
                Thread.Sleep(50);
            }
        }

        public void Stop()
        {
            if (_stopRequested || !IsRunning)
                return;
            lock (_threadLock)
            {
                _stopRequested = true;
                _watchThread.Join();
                _stopRequested = false;
                _isRunning = false;
                _watchThread = null;
            }
        }


        public void RemoveProcess(string processFilePath)
        {
            lock (_processesLock)
            {
                if (_processNames.Contains(processFilePath.ToUpperInvariant()))
                {
                    _processNames.Remove(processFilePath.ToUpperInvariant());
                }
            }
        }

        private bool GetIsOneProcessRunning()
        {
           Process[] processes =   Process.GetProcesses();
      
           lock (_processesLock)
            {
                foreach(string processName in _processNames)
                {
                    if (processes.Any(p => p.ProcessName.ToUpperInvariant().Equals(processName)))
                        return true;
                }
            }
            return false;
        }

        private bool GetIsOneProcessFocused()
        {
            string currentProcessName = GetForegroundProcessName().ToUpperInvariant();
            bool result = false;
            lock (_processesLock)
            {
                if (_processNames.Contains(currentProcessName))
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
