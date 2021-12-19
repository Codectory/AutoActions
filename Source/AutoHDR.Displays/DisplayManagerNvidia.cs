using CCD.Enum;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace AutoHDR.Displays
{
    public class DisplayManagerNvidia : DisplayManagerBase
    {
        public override GraphicsCardType GraphicsCardType => GraphicsCardType.NVIDIA;

        internal DisplayManagerNvidia() :base()
        {
            NVIDIA.Initialize();

        }

        public HDRColorData GetHDRColorData(bool enableHDR)
        {
            NvAPIWrapper.Native.Display.ColorDataHDRMode hdrMode = enableHDR ? NvAPIWrapper.Native.Display.ColorDataHDRMode.UHDA : NvAPIWrapper.Native.Display.ColorDataHDRMode.Off;
            return  new HDRColorData(hdrMode, default, NvAPIWrapper.Native.Display.ColorDataFormat.Auto, NvAPIWrapper.Native.Display.ColorDataDynamicRange.Auto);

        }

        protected override void ActivateHDR(Display display)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag);
            nvidiaDisplay.SetHDRColorData(GetHDRColorData(true));
        }

        protected override void DeactivateHDR(Display display)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag );
            nvidiaDisplay.SetHDRColorData(GetHDRColorData(false));
        }

        public override List<Display> GetActiveMonitors()
        {
            List<Display> displays = new List<Display>();
            NvAPIWrapper.Display.Display[] nvidiaDisplays = NvAPIWrapper.Display.Display.GetDisplays();
            DisplayDevice primaryDevice = DisplayDevice.GetGDIPrimaryDisplayDevice();
            for (int i = 0; i < nvidiaDisplays.Length; i++)
            {
                DisplayDevice displayDevice = nvidiaDisplays[i].DisplayDevice;
                if (displayDevice.IsActive)
                {
                    uint id = (uint)i;
                    uint uid = GetUID(id);
                    bool isPrimary = displayDevice.DisplayId == primaryDevice.DisplayId;
                    string name = nvidiaDisplays[i].Name;
                    string graphicsCard = displayDevice.Output.PhysicalGPU.FullName;
                    Display display = new Display(id, uid, isPrimary, name, graphicsCard);
                    display.Tag = displayDevice.DisplayId;
                    display.Resolution = GetResolution(display);
                    display.RefreshRate = GetRefreshRate(display);
                    display.ColorDepth = GetColorDepth(display);
                    displays.Add(display);

                }
            }
            return displays;
        }

        public override bool GetHDRState(Display display)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag);
            return nvidiaDisplay.HDRColorData.HDRMode != NvAPIWrapper.Native.Display.ColorDataHDRMode.Off;
        }

        public override int GetRefreshRate(Display display)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag);
            return nvidiaDisplay.CurrentTiming.Extra.RefreshRate;
        }

        public override void SetRefreshRate(Display display, int refreshRate)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag);
            nvidiaDisplay.Output.OverrideRefreshRate(refreshRate);
        }

        public override Size GetResolution(Display display)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag);
            return new Size((int)nvidiaDisplay.ScanOutInformation.TargetDisplayWidth, (int)nvidiaDisplay.ScanOutInformation.TargetDisplayHeight);
        }

        public override uint GetUID(uint displayID)
        {
            return HDRController.GetUID(displayID);
        }

        public override ColorDepth GetColorDepth(Display display)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag);
            return nvidiaDisplay.CurrentColorData.ColorDepth.ConvertToColorDepth();

        }
        public override void SetColorDepth(Display display, ColorDepth colorDepth)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag);
            NvAPIWrapper.Native.Display.ColorDataDepth nvColorDepth = colorDepth.ConvertNvidiaColorDepth();
            ColorData colorData = new ColorData(nvidiaDisplay.CurrentColorData.ColorFormat, nvidiaDisplay.CurrentColorData.Colorimetry, nvidiaDisplay.CurrentColorData.DynamicRange, NvAPIWrapper.Native.Display.ColorDataDepth.BPC8, nvidiaDisplay.CurrentColorData.SelectionPolicy, nvidiaDisplay.CurrentColorData.DesktopColorDepth);
            nvidiaDisplay.SetColorData(colorData);
        }



        public override void SetResolution(Display display, Size resolution)
        {

        }


    }

}
