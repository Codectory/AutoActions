using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioSwitcher.AudioApi
{
    public interface IAudioController : IDisposable
    {
        IDevice DefaultPlaybackDevice { get; set; }

        IDevice DefaultPlaybackCommunicationsDevice { get; set; }

        IDevice DefaultCaptureDevice { get; set; }

        IDevice DefaultCaptureCommunicationsDevice { get; set; }

        event EventHandler<DeviceChangedEventArgs> AudioDeviceChanged;

        IDevice GetDevice(Guid id);

        Task<IDevice> GetDeviceAsync(Guid id);

        IDevice GetDevice(Guid id, DeviceState state);

        Task<IDevice> GetDeviceAsync(Guid id, DeviceState state);

        IDevice GetDefaultDevice(DeviceType deviceType, Role role);

        Task<IDevice> GetDefaultDeviceAsync(DeviceType deviceType, Role role);

        IEnumerable<IDevice> GetDevices();

        Task<IEnumerable<IDevice>> GetDevicesAsync();

        IEnumerable<IDevice> GetDevices(DeviceState state);

        Task<IEnumerable<IDevice>> GetDevicesAsync(DeviceState state);

        IEnumerable<IDevice> GetDevices(DeviceType deviceType);

        Task<IEnumerable<IDevice>> GetDevicesAsync(DeviceType deviceType);

        IEnumerable<IDevice> GetDevices(DeviceType deviceType, DeviceState state);

        Task<IEnumerable<IDevice>> GetDevicesAsync(DeviceType deviceType, DeviceState state);

        IEnumerable<IDevice> GetPlaybackDevices();

        IEnumerable<IDevice> GetPlaybackDevices(DeviceState deviceState);

        Task<IEnumerable<IDevice>> GetPlaybackDevicesAsync();

        Task<IEnumerable<IDevice>> GetPlaybackDevicesAsync(DeviceState deviceState);

        IEnumerable<IDevice> GetCaptureDevices();

        IEnumerable<IDevice> GetCaptureDevices(DeviceState deviceState);

        Task<IEnumerable<IDevice>> GetCaptureDevicesAsync();

        Task<IEnumerable<IDevice>> GetCaptureDevicesAsync(DeviceState deviceState);

        bool SetDefaultDevice(IDevice dev);

        bool SetDefaultCommunicationsDevice(IDevice dev);

        Task<bool> SetDefaultDeviceAsync(IDevice dev);

        Task<bool> SetDefaultCommunicationsDeviceAsync(IDevice dev);
    }
}