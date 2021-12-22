namespace AudioSwitcher.AudioApi.CoreAudio.Interfaces
{
    /// <summary>
    ///     MMDevice STGM enumeration
    /// </summary>
    internal enum StorageAccessMode : uint
    {
        Read = 0x00000000,
        Write = 0x00000001,
        ReadWrite = 0x00000002
    }
}