using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodectoryCore;
using Microsoft.Win32;

namespace AutoHDR.Threading
{
    public class ThreadManager
    {
        public DateTime _lastWakeup = DateTime.MinValue;
        public bool IsActive { get; private set; } = false;

        readonly object _lockThreadAcces = new object();

        BlockingCollection<IManagedThread> _managedThreds = new BlockingCollection<IManagedThread>();

        internal List<IManagedThread> ManagedThreds => _managedThreds.ToList();

        public ThreadManager()
        {
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
           if (e.Mode == PowerModes.Suspend)
                Sleep();
           else if (e.Mode == PowerModes.Resume)
                WakeUp();
        }

        private void WakeUp()
        {
            lock (_lockThreadAcces)
            {
                _lastWakeup = DateTime.Now;
                if (IsActive)
                    InternalStartThreads();
            }
        }

        private void Sleep()
        {
            lock (_lockThreadAcces)
            {
                InternalStopThreads();
            }
        }

        public void Add(IManagedThread thread)
        {
            lock (_lockThreadAcces)
            {
                if (!_managedThreds.TryAdd(thread, 1000))
                    throw new Exception("Couldn't add thread.");
            }
        }

        public void Remove(IManagedThread thread)
        {
            lock (_lockThreadAcces)
            {
                if (!_managedThreds.TryTake(out thread,1000))
                    throw new Exception("Couldn't remove thread.");
            }
        }


        public void StartThreads()
        {
            while ((DateTime.Now - _lastWakeup).TotalSeconds <= 5)
                System.Threading.Thread.Sleep(100);
            lock (_lockThreadAcces)
            {
                if (IsActive)
                    return;
                InternalStartThreads();
                IsActive = true;
            }
        }

        private void InternalStartThreads()
        {
            foreach (IManagedThread thread in ManagedThreds)
                if (!thread.ManagedThreadIsActive)
                    thread.StartManagedThread();
        }

        public void StopThreads()
        {
            lock (_lockThreadAcces)
            {
                if (!IsActive)
                    return;
                InternalStopThreads();
                IsActive = false;
            }
        }
        private void InternalStopThreads()
        {
            foreach (IManagedThread thread in ManagedThreds)
                if (thread.ManagedThreadIsActive)
                    thread.StopManagedThread();

        }
    }
}
