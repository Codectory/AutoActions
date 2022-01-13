namespace AudioSwitcher.AudioApi
{
    public sealed class DeviceStateChangedEventArgs : DeviceChangedEventArgs
    {
        public DeviceState State { get; private set; }

        public DeviceStateChangedEventArgs(IDevice dev, DeviceState state)
            : base(dev, AudioDeviceEventType.StateChanged)
        {
            State = state;
        }
    }
}
