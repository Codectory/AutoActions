using CCD.Enum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace AutoHDR.Displays
{
    public class DisplayManagerGeneric : DisplayManagerBase
    {
        public override GraphicsCardType GraphicsCardType => GraphicsCardType.OTHER;

        internal DisplayManagerGeneric() : base()
        {

        }

        protected override void ActivateHDR(Display display)
        { 
            HDRController.SetHDRState(display.UID, true);

        }

        protected override void DeactivateHDR(Display display)
        {
            HDRController.SetHDRState(display.UID, false);
        }

 

        public override List<Display> GetActiveMonitors()
        {
            List<Display> displays = new List<Display>();
            DisplayConfigTopologyId topologyId;
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

        public override ColorDepth GetColorDepth(Display display)
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

        public override bool GetHDRState(Display display)
        {
            return HDRController.GetHDRState(display.UID);
        }

        public override int GetRefreshRate(Display display)
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

        public override Size GetResolution(Display display)
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

        public override uint GetUID(uint displayID)
        {
            return HDRController.GetUID(displayID);
        }

        public override void SetColorDepth(Display display, ColorDepth colorDepth)
        {
            throw new NotImplementedException();
        }

        public override void SetRefreshRate(Display display, int refreshRate)
        {
            Func<DEVMODE, DEVMODE> func = (dm) =>
            {
                dm.dmDisplayFrequency = refreshRate;

                return dm;
            };
            ChangeDisplaySetting(display.ID, func);
        }

        public override void SetResolution(Display display, Size resolution)
        {
            Func<DEVMODE, DEVMODE> func = (dm) =>
            {
                dm.dmPelsHeight = Convert.ToInt32(resolution.Height);
                dm.dmPelsWidth = Convert.ToInt32(resolution.Width);
                return dm;
            };
            ChangeDisplaySetting(display.ID, func);
        }

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
