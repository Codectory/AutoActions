using System;
using System.Threading.Tasks;

namespace AudioSwitcher.AudioApi
{
    public interface IDevice
    {
        IAudioController Controller { get; }

        Guid Id { get; }

        string Name { get; }

        string InterfaceName { get; }

        string FullName { get; }

        DeviceIcon Icon { get; }

        bool IsDefaultDevice { get; }

        bool IsDefaultCommunicationsDevice { get; }

        DeviceState State { get; }

        DeviceType DeviceType { get; }

        bool IsPlaybackDevice { get; }

        bool IsCaptureDevice { get; }

        bool IsMuted { get; }

        int Volume { get; set; }

        bool SetAsDefault();

        Task<bool> SetAsDefaultAsync();

        bool SetAsDefaultCommunications();

        Task<bool> SetAsDefaultCommunicationsAsync();

        bool Mute(bool mute);

        Task<bool> MuteAsync(bool mute);

        bool ToggleMute();

        Task<bool> ToggleMuteAsync();

        event EventHandler<DeviceChangedEventArgs> VolumeChanged;

        [Obsolete("Use Mute(true) instead")]
        bool Mute();

        [Obsolete("Use MuteAsync(true) instead")]
        Task<bool> MuteAsync();

        [Obsolete("Use Mute(false) instead")]
        bool UnMute();

        [Obsolete("Use MuteAsync(false) instead")]
        Task<bool> UnMuteAsync();
    }
}