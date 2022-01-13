using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Displays
{
    public enum ColorDepth
    {
        BPC16 = 16,
        BPC12 = 12,
        BPC10 = 10,
        BPC8 = 8,
        BPC6 = 6,
        BPCUnkown = 0
    }

    public static class ColorDepthExcetion
    {
        public static NvAPIWrapper.Native.Display.ColorDataDepth ConvertNvidiaColorDepth(this ColorDepth colorDepth)
        {
            NvAPIWrapper.Native.Display.ColorDataDepth nvColorDepth;

            switch (colorDepth)
            {
                case ColorDepth.BPC16:
                    nvColorDepth = NvAPIWrapper.Native.Display.ColorDataDepth.BPC16;
                    break;
                case ColorDepth.BPC12:
                    nvColorDepth = NvAPIWrapper.Native.Display.ColorDataDepth.BPC12;
                    break;
                case ColorDepth.BPC10:
                    nvColorDepth = NvAPIWrapper.Native.Display.ColorDataDepth.BPC10;
                    break;
                case ColorDepth.BPC8:
                    nvColorDepth = NvAPIWrapper.Native.Display.ColorDataDepth.BPC8;
                    break;
                case ColorDepth.BPC6:
                    nvColorDepth = NvAPIWrapper.Native.Display.ColorDataDepth.BPC6;
                    break;
                default:
                    nvColorDepth = NvAPIWrapper.Native.Display.ColorDataDepth.Default;
                    break;
            }
            return nvColorDepth;
        }

        public static ColorDepth ConvertToColorDepth(this NvAPIWrapper.Native.Display.ColorDataDepth? nvidiaColorDepth)
        {
            ColorDepth colorDepth;

            switch (nvidiaColorDepth)
            {
                case NvAPIWrapper.Native.Display.ColorDataDepth.BPC16:
                    colorDepth = ColorDepth.BPC16;
                    break;
                case NvAPIWrapper.Native.Display.ColorDataDepth.BPC12:
                    colorDepth = ColorDepth.BPC12;
                    break;
                case NvAPIWrapper.Native.Display.ColorDataDepth.BPC10:
                    colorDepth = ColorDepth.BPC10;
                    break;
                case NvAPIWrapper.Native.Display.ColorDataDepth.BPC8:
                    colorDepth = ColorDepth.BPC8;
                    break;
                case NvAPIWrapper.Native.Display.ColorDataDepth.BPC6:
                    colorDepth = ColorDepth.BPC6;
                    break;
                default:
                    colorDepth = ColorDepth.BPCUnkown;
                    break;
            }
            return colorDepth;
        }
    }
}
