using System;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    [Flags]
    internal enum ERole : uint
    {
        Console = 0,
        Multimedia = 1,
        Communications = 2
    }
}