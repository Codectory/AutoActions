using System;
using System.Runtime.InteropServices;

namespace AudioSwitcher.AudioApi.CoreAudio.Interfaces
{
    [Guid(ComIIds.AUDIO_ENDPOINT_VOLUME_EX_IID)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolumeEx
    {
        [PreserveSig]
        int RegisterControlChangeNotify(
            [In] IAudioEndpointVolumeCallback client);

        [PreserveSig]
        int UnregisterControlChangeNotify(
            [In] IAudioEndpointVolumeCallback clientCallback);

        [PreserveSig]
        int GetChannelCount(
            [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 channelCount);

        [PreserveSig]
        int SetMasterVolumeLevel(
            [In] [MarshalAs(UnmanagedType.R4)] float level,
            [In] ref Guid eventContext);

        [PreserveSig]
        int SetMasterVolumeLevelScalar(
            [In] [MarshalAs(UnmanagedType.R4)] float level,
            [In] ref Guid eventContext);

        [PreserveSig]
        int GetMasterVolumeLevel(
            [Out] [MarshalAs(UnmanagedType.R4)] out float level);

        [PreserveSig]
        int GetMasterVolumeLevelScalar(
            [Out] [MarshalAs(UnmanagedType.R4)] out float level);

        [PreserveSig]
        int SetChannelVolumeLevel(
            [In] [MarshalAs(UnmanagedType.SysUInt)] UIntPtr channelNumber,
            [In] [MarshalAs(UnmanagedType.R4)] float level,
            [In] ref Guid eventContext);

        [PreserveSig]
        int SetChannelVolumeLevelScalar(
            [In] [MarshalAs(UnmanagedType.SysUInt)] UIntPtr channelNumber,
            [In] [MarshalAs(UnmanagedType.R4)] float level,
            [In] ref Guid eventContext);

        [PreserveSig]
        int GetChannelVolumeLevel(
            [In] [MarshalAs(UnmanagedType.SysUInt)] UIntPtr channelNumber,
            [Out] [MarshalAs(UnmanagedType.R4)] out float level);

        [PreserveSig]
        int GetChannelVolumeLevelScalar(
            [In] [MarshalAs(UnmanagedType.SysUInt)] UIntPtr channelNumber,
            [Out] [MarshalAs(UnmanagedType.R4)] out float level);

        [PreserveSig]
        int SetMute(
            [In] [MarshalAs(UnmanagedType.Bool)] Boolean isMuted,
            [In] ref Guid eventContext);

        [PreserveSig]
        int GetMute(
            [Out] [MarshalAs(UnmanagedType.Bool)] out Boolean isMuted);

        [PreserveSig]
        int GetVolumeStepInfo(
            [Out] [MarshalAs(UnmanagedType.SysUInt)] out UIntPtr step,
            [Out] [MarshalAs(UnmanagedType.SysUInt)] out UIntPtr stepCount);

        [PreserveSig]
        int VolumeStepUp(
            [In] ref Guid eventContext);

        [PreserveSig]
        int VolumeStepDown(
            [In] ref Guid eventContext);

        [PreserveSig]
        int QueryHardwareSupport(
            [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 hardwareSupportMask);

        [PreserveSig]
        int GetVolumeRange(
            [Out] [MarshalAs(UnmanagedType.R4)] out float volumeMin,
            [Out] [MarshalAs(UnmanagedType.R4)] out float volumeMax,
            [Out] [MarshalAs(UnmanagedType.R4)] out float volumeStep);

        [PreserveSig]
        int GetVolumeRangeChannel(
            [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
            [Out] [MarshalAs(UnmanagedType.R4)] out float volumeMin,
            [Out] [MarshalAs(UnmanagedType.R4)] out float volumeMax,
            [Out] [MarshalAs(UnmanagedType.R4)] out float volumeStep);
    }
}
