namespace AudioSwitcher.AudioApi
{
    /// <summary>
    ///     The type of change raised
    /// </summary>
    public enum AudioDeviceEventType
    {
        DefaultDevice,
        DefaultCommunicationsDevice,
        Added,
        Removed,
        Volume,
        PropertyChanged,
        StateChanged
    }
}