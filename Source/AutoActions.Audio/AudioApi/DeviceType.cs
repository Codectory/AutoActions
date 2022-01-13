using System;

namespace AudioSwitcher.AudioApi
{
    [Flags]
    public enum DeviceType
    {
        Playback = 0x00000001,
        Capture = 0x00000002,
        All = Playback | Capture
    };
}