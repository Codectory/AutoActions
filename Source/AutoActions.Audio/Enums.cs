namespace AutoActions.Audio
{
    public enum AudioDeviceType
    {
        Playback,
        Capture,
        All,
        Unknown 
    }

    public enum DeviceState
    {
        Active = 1,
        Disabled = 2,
        NotPresent = 4,
        Unplugged = 8,
        All = 15
    }
}