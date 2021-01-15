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
        [DllImport("HDRController.dll")]
        private static extern IntPtr SetHDR(bool enabled);
        readonly static object _dllLock = new object();

        public static void ActivateHDR()
        {
            lock (_dllLock)
                SetHDR(true);

        }

        public static  void DeactivateHDR()
        {
            lock (_dllLock)
                SetHDR(false);
        }
    }
}
