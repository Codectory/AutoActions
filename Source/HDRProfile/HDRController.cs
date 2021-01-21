using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HDRProfile
{
   public static class HDRController
    {

        static Thread _updateThread = null;
        static readonly object _threadControlLock = new object();
        static bool _monitorCancelRequested = false;

        readonly static object _dllLock = new object();

        [DllImport("HDRController.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SetHDRState(bool enabled);

        [DllImport("HDRController.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GetHDRState();


        public static bool HDRIsActive { get; private set; } = false;
        public static bool Monitoring { get; private set; } = false;

        public static event EventHandler HDRIsActiveChanged;

        public static void StartMonitoring()
        {
            lock (_threadControlLock)
            {
                if (Monitoring)
                    return;
                _updateThread = new Thread(HDRMonitorLoop);
                _updateThread.IsBackground = true;
                Monitoring = true;
                _monitorCancelRequested = false;
                _updateThread.Start();
            }
        }

        public static void StopMonitoring()
        {
            lock (_threadControlLock)
            {
                if (!Monitoring)
                    return;
                _monitorCancelRequested = true;
                _updateThread.Join();
                _updateThread = null;
                Monitoring = false;
            }
        }

        private static void HDRMonitorLoop()
        {
            while (!_monitorCancelRequested)
            {
                bool currentValue = GetHDRState();
                bool changed = HDRIsActive != currentValue;
                HDRIsActive = currentValue;
                if (changed)
                {
                    try {HDRIsActiveChanged?.Invoke(null, EventArgs.Empty);} catch {}                
                }
                System.Threading.Thread.Sleep(50);
            }
        }

        public static void ActivateHDR()
        {
            lock (_dllLock)
                SetHDRState(true);
        }

        public static  void DeactivateHDR()
        {
            lock (_dllLock)
                SetHDRState(false);
        }
    }
}
