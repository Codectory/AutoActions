using AutoHDR.Threading;
using CCD;
using CCD.Enum;
using CCD.Struct;
using CodectoryCore;
using CodectoryCore.UI.Wpf;
using NvAPIWrapper.Display;
using NvAPIWrapper.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace AutoHDR.Displays
{
    public class DisplayManagerHandler
    {
        public static GraphicsCardType GraphicsCardType => Instance.GraphicsCardType;


        private static IDisplayManagerBase _instance = null;

        public static IDisplayManagerBase Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        NvAPIWrapper.NVIDIA.Initialize();
                        if (NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs().Count() > 0)
                        {
                            _instance = new DisplayManagerNvidia();
                        }
                        else
                        {
                            _instance = new DisplayManagerGeneric();
                        }
                    }
                    catch (Exception)
                    {
                        _instance = new DisplayManagerGeneric();
                    }
                }
                return _instance;
            }
        }

    }
}
