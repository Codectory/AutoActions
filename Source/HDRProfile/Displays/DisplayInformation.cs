using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Displays
{
    public class DisplayInformation
    {
        public uint Id { get; private set; }
        public DISPLAY_DEVICE DisplayDevice { get; private set; }
        public DEVMODE Devmode { get; private set; }
        public bool IsPrimary => DisplayDevice.StateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice);

        public DisplayInformation(uint id, DISPLAY_DEVICE displayDevice, DEVMODE devmode) : this(id, displayDevice)
        {
            Devmode = devmode;
        }

        public DisplayInformation(uint id, DISPLAY_DEVICE displayDevice)
        {
            Id = id;
            DisplayDevice = displayDevice;
        }
    }
}
