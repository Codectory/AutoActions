using System.Runtime.InteropServices;

namespace AudioSwitcher.AudioApi.CoreAudio.Interfaces
{
    [Guid(ComIIds.IMM_NOTIFICATION_CLIENT_IID)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMNotificationClient
    {
        void OnDeviceStateChanged(
            [MarshalAs(UnmanagedType.LPWStr)] string deviceId,
            [MarshalAs(UnmanagedType.U4)] EDeviceState newState);

        void OnDeviceAdded(
            [MarshalAs(UnmanagedType.LPWStr)] string deviceId);

        void OnDeviceRemoved(
            [MarshalAs(UnmanagedType.LPWStr)] string deviceId);

        void OnDefaultDeviceChanged(
            [MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
            [MarshalAs(UnmanagedType.I4)] ERole deviceRole,
            [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);

        void OnPropertyValueChanged(
            [MarshalAs(UnmanagedType.LPWStr)] string deviceId, PropertyKey propertyKey);
    }
}
