using System.Runtime.InteropServices;

namespace AudioSwitcher.AudioApi.CoreAudio.Interfaces
{
    [Guid(ComIIds.AUDIO_METER_INFORMATION_IID)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioMeterInformation
    {
        [PreserveSig]
        int GetPeakValue([Out] [MarshalAs(UnmanagedType.R4)] out float peak);

        [PreserveSig]
        int GetMeteringChannelCount([Out] [MarshalAs(UnmanagedType.U4)] out uint channelCount);

        [PreserveSig]
        int GetChannelsPeakValues(
            [In] [MarshalAs(UnmanagedType.U4)] uint channelCount,
            [In, Out] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[] peakValues);

        [PreserveSig]
        int QueryHardwareSupport([Out] [MarshalAs(UnmanagedType.U4)] out uint hardwareSupportMask);
    }
}
