namespace AudioSwitcher.AudioApi
{
    public sealed class DeviceAddedEventArgs : DeviceChangedEventArgs
    {
        public DeviceAddedEventArgs(IDevice dev)
            : base(dev, AudioDeviceEventType.Added)
        {
        }
    }
}
