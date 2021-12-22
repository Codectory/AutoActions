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

        public  List<Display> GetActiveMonitors2()
        {
            List<Display> displays = new List<Display>();
            DisplayHandle[] handles = DisplayApi.EnumNvidiaDisplayHandle();
            IPathInfo[] config = DisplayApi.GetDisplayConfig();
            for (int i = 0; i < handles.Length; i++)
            {
                string displayName = DisplayApi.GetAssociatedNvidiaDisplayName(handles[i]);
                uint displayID = DisplayApi.GetDisplayIdByDisplayName(displayName);
                IPathInfo pathInfo = config.First(p => p.TargetsInfo.ToList().First().DisplayId == displayID);

                DisplayDevice displayDevice = new DisplayDevice(displayID);
                if (displayDevice.IsActive)
                {
                    uint id = pathInfo.SourceId;
                    uint uid = 0;
                    if (Displays.Any(m => m.Tag != null && displayDevice.DisplayId.Equals(((DisplayDevice)m.Tag).DisplayId)))
                        uid = Displays.First(m => displayDevice.DisplayId.Equals(((DisplayDevice)m.Tag).DisplayId)).UID;
                    else
                        uid = GetUID(id);
                    bool isPrimary = pathInfo.SourceModeInfo.IsGDIPrimary;
                    string name = displayName;
                    string graphicsCard = displayDevice.Output.PhysicalGPU.FullName;
                    Display display = new Display(id, uid, isPrimary, name, graphicsCard);
                    display.Tag = displayDevice;
                    display.Resolution =  new Size(displayDevice.CurrentTiming.HorizontalActive, displayDevice.CurrentTiming.VerticalActive);
                    display.RefreshRate = GetRefreshRate(display);
                    display.ColorDepth = GetColorDepth(display);
                    displays.Add(display);

                }
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
