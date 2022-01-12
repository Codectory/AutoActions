using CCD.Enum;
using NvAPIWrapper;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.Display.Structures;
using NvAPIWrapper.Native.Interfaces.Display;
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
        public override List<Display> GetActiveMonitors()
        { 
            var displays = base.GetActiveMonitors();
            foreach (var display in displays)
            {
                display.Tag = NvAPIWrapper.Native.DisplayApi.GetDisplayIdByDisplayName(display.Name);
            }
            return displays;
        }


        public override void SetColorDepth(Display display, ColorDepth colorDepth)
        {
            DisplayDevice nvidiaDisplay = new DisplayDevice((uint)display.Tag);
            NvAPIWrapper.Native.Display.ColorDataDepth nvColorDepth = colorDepth.ConvertNvidiaColorDepth();
            ColorData colorData = new ColorData(nvidiaDisplay.CurrentColorData.ColorFormat, nvidiaDisplay.CurrentColorData.Colorimetry, nvidiaDisplay.CurrentColorData.DynamicRange, nvColorDepth, nvidiaDisplay.CurrentColorData.SelectionPolicy, nvidiaDisplay.CurrentColorData.DesktopColorDepth);
            nvidiaDisplay.SetColorData(colorData);
        }
    }

}
