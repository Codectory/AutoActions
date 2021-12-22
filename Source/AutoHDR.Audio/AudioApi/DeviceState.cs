using System;

namespace AudioSwitcher.AudioApi
{
    [Flags]
    public enum DeviceState
    {
        Active = 0x00000001,
        Disabled = 0x00000002,
        NotPresent = 0x00000004,
        Unplugged = 0x00000008,
        All = Active | Disabled | NotPresent | Unplugged
    }
}