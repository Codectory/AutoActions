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
   public static  class HDRController
    {
        [DllImport("HDRController.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SetHDRState(bool enabled);

        [DllImport("HDRController.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GetHDRState();

        readonly static object _dllLock = new object();

        public static bool HDRIsActive()
        {
            return GetHDRState();
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
