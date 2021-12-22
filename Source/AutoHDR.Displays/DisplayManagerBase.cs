using AutoHDR.Threading;
using CCD;
using CCD.Enum;
using CCD.Struct;
using CodectoryCore;
using CodectoryCore.UI.Wpf;
using Microsoft.Win32;
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
    public abstract class DisplayManagerBase : BaseViewModel, IDisplayManagerBase
    {
        public abstract GraphicsCardType GraphicsCardType { get; }
        bool _selectedHDR = false;

        readonly object _lockUpdateDisplays = new object();

        readonly object _lockExceptionThrown = new object();
        public EventHandler<Exception> _exceptionThrown;

        event EventHandler<Exception> IDisplayManagerBase.ExceptionThrown
        {
            add
            {
                lock (_lockExceptionThrown)
                {
                    _exceptionThrown += value;
                }
            }

            remove
            {
                lock (_lockExceptionThrown)
                {
                    _exceptionThrown -= value;
                }
            }
        }

        public bool GlobalHDRIsActive { get; private set; } = false;
        public bool SelectedHDR { get => _selectedHDR; set { _selectedHDR = value; OnPropertyChanged(); } } 

        public event EventHandler HDRIsActiveChanged;

        private DispatchingObservableCollection<Display> _monitors = new DispatchingObservableCollection<Display>();
        public DispatchingObservableCollection<Display> Displays { get => _monitors; set { _monitors = value; OnPropertyChanged(); } }

        protected DisplayManagerBase()
        {
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            UpdateDisplays();
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            UpdateDisplays();
        }

        public void LoadKnownDisplays(List<Display> knownMonitors)
        {
            //foreach (var monitor in knownMonitors)
            //    Displays.Add(monitor);
            MergeMonitors(knownMonitors);

            MergeMonitors(GetActiveMonitors());
        }

        public void UpdateDisplays()
        {
            lock (_lockUpdateDisplays)
            {
                bool currentValue = false;
                MergeMonitors(GetActiveMonitors());
                foreach (Display monitor in Displays)
                {
                    monitor.UpdateHDRState();
                    if (monitor.Managed)
                        currentValue = currentValue || monitor.HDRState;
                }
                bool changed = GlobalHDRIsActive != currentValue;
                GlobalHDRIsActive = currentValue;
                if (changed)
                {
                    try { HDRIsActiveChanged?.Invoke(null, EventArgs.Empty); } catch { }
                }
            }
        }

        public void ActivateHDR(Display display)
        {
            HDRController.SetHDRState(display.UID, true);

        }

        public void DeactivateHDR(Display display)
        {
            HDRController.SetHDRState(display.UID, false);
        }

        public void ActivateHDR()
        {
            if (!SelectedHDR)
                HDRController.SetGlobalHDRState(true);
            else
            {
                foreach (Display display in Displays)
                    if (display.Managed)
                        ActivateHDR(display);
            }
        }

        public void DeactivateHDR()
        {
            if (!SelectedHDR)
                HDRController.SetGlobalHDRState(false);
            else
            {
                foreach (Display display in Displays)
                    if (display.Managed)
                        DeactivateHDR(display);

            }
        }

        public virtual List<Display> GetActiveMonitors()
        {
            List<Display> displays = new List<Display>();
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);

            uint displayID = 0;
            while (NativeMethods.EnumDisplayDevices(null, displayID, ref d, 0))
            {
                DisplayInformation displayInfo;
                if (0 != NativeMethods.EnumDisplaySettings(d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
                    displayInfo = new DisplayInformation(displayID, d, dm);
                else
                    displayInfo = new DisplayInformation(displayID, d);
                Display display = new Display(displayInfo, GetUID(displayID));
                display.ColorDepth = (ColorDepth)HDRController.GetColorDepth(display.UID);
                if (!displays.Any(m => m.ID.Equals(display.ID)) && display.UID != 0)
                    displays.Add(display);
                displayID++;
            }
            return displays;
        }

        public ColorDepth GetColorDepth(Display display)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);


            NativeMethods.EnumDisplayDevices(null, display.ID, ref d, 0);

            if (0 != NativeMethods.EnumDisplaySettings(
                d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
            {

                return (ColorDepth)dm.dmBitsPerPel;
            }
            return ColorDepth.BPCUnkown;
        }

        public bool GetHDRState(Display display)
        {
            return HDRController.GetHDRState(display.UID);
        }

        public int GetRefreshRate(Display display)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);

            NativeMethods.EnumDisplayDevices(null, display.ID, ref d, 0);

            if (0 != NativeMethods.EnumDisplaySettings(
                d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
            {
                return dm.dmDisplayFrequency;

            }
            else return 0;
        }

        public Size GetResolution(Display display)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);


            NativeMethods.EnumDisplayDevices(null, display.ID, ref d, 0);

            if (0 != NativeMethods.EnumDisplaySettings(
                d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
            {
                Size resolution = new Size(dm.dmPelsWidth, dm.dmPelsHeight);
                return resolution;

            }
            else return Size.Empty;
        }

        public uint GetUID(uint displayID)
        {
            return HDRController.GetUID(displayID);
        }



        private void MergeMonitors(List<Display> activeMonitors)
        {

            try
            {
                List<Display> toRemove = new List<Display>();
                foreach (Display monitor in Displays)
                {
                    if (monitor.UID == 0 || !activeMonitors.Any(m => m.UID.Equals(monitor.UID)))
                        toRemove.Add(monitor);
                }
                foreach (Display monitor in toRemove)
                    Displays.Remove(monitor);
                foreach (Display monitor in activeMonitors)
                {
                    if (monitor.UID != 0 && !Displays.Any(m => m.UID.Equals(monitor.UID)))
                        Displays.Add(monitor);
                    else
                    {
                        Display existingMonitor = Displays.First(m => m.UID.Equals(monitor.UID));
                        existingMonitor.Name = monitor.Name;
                        existingMonitor.ColorDepth = monitor.ColorDepth;
                        existingMonitor.RefreshRate = monitor.RefreshRate;
                        existingMonitor.Resolution = monitor.Resolution;
                        existingMonitor.GraphicsCard = monitor.GraphicsCard;
                        existingMonitor.ID = monitor.ID;
                        existingMonitor.IsPrimary = monitor.IsPrimary;
                        existingMonitor.Resolution = monitor.Resolution;
                        existingMonitor.Tag = monitor.Tag;


                    }

                }
            }
            catch (Exception ex)
            {
                _exceptionThrown?.BeginInvoke(this, ex, null, null);
            }
        }

        public void SetRefreshRate(Display display, int refreshRate)
        {
            Func<DEVMODE, DEVMODE> func = (dm) =>
            {
                dm.dmDisplayFrequency = refreshRate;

                return dm;
            };
            ChangeDisplaySetting(display.ID, func);
        }

        public void SetResolution(Display display, Size resolution)
        {
            Func<DEVMODE, DEVMODE> func = (dm) =>
            {
                dm.dmPelsHeight = Convert.ToInt32(resolution.Height);
                dm.dmPelsWidth = Convert.ToInt32(resolution.Width);
                return dm;
            };
            ChangeDisplaySetting(display.ID, func);
        }


        public abstract void SetColorDepth(Display display, ColorDepth colorDepth);

        private void ChangeDisplaySetting(uint deviceID, Func<DEVMODE, DEVMODE> func)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);


            NativeMethods.EnumDisplayDevices(null, deviceID, ref d, 0);

            if (0 != NativeMethods.EnumDisplaySettings(
                d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
            {

                dm = func.Invoke(dm);

                DISP_CHANGE iRet = NativeMethods.ChangeDisplaySettingsEx(
                    d.DeviceName, ref dm, IntPtr.Zero,
                    DisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
            }
        }

    }
}
