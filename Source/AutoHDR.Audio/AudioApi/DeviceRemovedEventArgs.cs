namespace AudioSwitcher.AudioApi
{
    public sealed class DeviceRemovedEventArgs : DeviceChangedEventArgs
    {
        public DeviceRemovedEventArgs(IDevice dev)
            : base(dev, AudioDeviceEventType.Removed)
        {
        }
    }
}
