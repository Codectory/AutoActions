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
   public  class HDRController
    {
        [DllImport("HDRController.dll")]
        public static extern IntPtr SetHDR(bool enabled);
        readonly object _dllLock = new object();

        public void ActivateHDR()
        {
            lock (_dllLock)
                SetHDR(true);

        }

        public void DeactivateHDR()
        {
            lock (_dllLock)
                SetHDR(false);
        }
    }
}
