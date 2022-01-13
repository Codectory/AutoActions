using CCD.Enum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace AutoActions.Displays
{
    public class DisplayManagerGeneric : DisplayManagerBase
    {
        public override GraphicsCardType GraphicsCardType => GraphicsCardType.OTHER;

        internal DisplayManagerGeneric() : base()
        {

        }

        public override void SetColorDepth(Display display, ColorDepth colorDepth)
        {
            throw new NotImplementedException();
        }



    }
}
