namespace AudioSwitcher.AudioApi
{
    public sealed class DeviceVolumeChangedEventArgs : DeviceChangedEventArgs
    {
        public int Volume { get; private set; }

        public DeviceVolumeChangedEventArgs(IDevice dev, int volume)
            : base(dev, AudioDeviceEventType.Volume)
        {
            Volume = volume;
        }
    }
}
