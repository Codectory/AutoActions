using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioSwitcher.AudioApi
{
    public abstract class AudioController : IAudioController
    {
        protected const DeviceState DEFAULT_DEVICE_STATE_FILTER =
            DeviceState.Active | DeviceState.Unplugged | DeviceState.Disabled;

        public event EventHandler<DeviceChangedEventArgs> AudioDeviceChanged;

        public virtual IDevice DefaultPlaybackDevice
        {
            get
            {
                return GetDefaultDevice(DeviceType.Playback, Role.Console | Role.Multimedia);
            }
            set
            {
                SetDefaultDevice(value);
            }
        }

        public virtual IDevice DefaultPlaybackCommunicationsDevice
        {
            get
            {
                return GetDefaultDevice(DeviceType.Playback, Role.Communications);
            }
            set
            {
                SetDefaultCommunicationsDevice(value);
            }
        }

        public virtual IDevice DefaultCaptureDevice
        {
            get
            {
                return GetDefaultDevice(DeviceType.Capture, Role.Console | Role.Multimedia);
            }
            set
            {
                SetDefaultDevice(value);
            }
        }

        public virtual IDevice DefaultCaptureCommunicationsDevice
        {
            get
            {
                return GetDefaultDevice(DeviceType.Capture, Role.Communications);
            }
            set
            {
                SetDefaultCommunicationsDevice(value);
            }
        }

        public abstract IDevice GetDevice(Guid id);

        public virtual Task<IDevice> GetDeviceAsync(Guid id)
        {
            return Task.Factory.StartNew(() => GetDevice(id));
        }

        public abstract IDevice GetDevice(Guid id, DeviceState state);

        public virtual Task<IDevice> GetDeviceAsync(Guid id, DeviceState state)
        {
            return Task.Factory.StartNew(() => GetDevice(id, state));
        }

        public abstract IDevice GetDefaultDevice(DeviceType deviceType, Role role);
        public virtual Task<IDevice> GetDefaultDeviceAsync(DeviceType deviceType, Role role)
        {
            return Task.Factory.StartNew(() => GetDefaultDevice(deviceType, role));
        }

        public virtual IEnumerable<IDevice> GetDevices()
        {
            return GetDevices(DEFAULT_DEVICE_STATE_FILTER);
        }

        public virtual Task<IEnumerable<IDevice>> GetDevicesAsync()
        {
            return Task.Factory.StartNew(() => GetDevices());
        }
        public virtual Task<IEnumerable<IDevice>> GetDevicesAsync(DeviceState state)
        {
            return Task.Factory.StartNew(() => GetDevices(state));
        }

        public IEnumerable<IDevice> GetDevices(DeviceType deviceType)
        {
            return GetDevices(deviceType, DEFAULT_DEVICE_STATE_FILTER);
        }

        public Task<IEnumerable<IDevice>> GetDevicesAsync(DeviceType deviceType)
        {
            return GetDevicesAsync(deviceType, DEFAULT_DEVICE_STATE_FILTER);
        }

        public virtual IEnumerable<IDevice> GetDevices(DeviceState state)
        {
            return GetDevices(DeviceType.All, state);
        }

        public abstract IEnumerable<IDevice> GetDevices(DeviceType deviceType, DeviceState state);
        public virtual Task<IEnumerable<IDevice>> GetDevicesAsync(DeviceType deviceType, DeviceState state)
        {
            return Task.Factory.StartNew(() => GetDevices(deviceType, state));
        }

        public abstract IEnumerable<IDevice> GetPlaybackDevices();
        public virtual Task<IEnumerable<IDevice>> GetPlaybackDevicesAsync()
        {
            return Task.Factory.StartNew(() => GetPlaybackDevices());
        }

        public abstract IEnumerable<IDevice> GetPlaybackDevices(DeviceState deviceState);
        public virtual Task<IEnumerable<IDevice>> GetPlaybackDevicesAsync(DeviceState deviceState)
        {
            return Task.Factory.StartNew(() => GetPlaybackDevices(deviceState));
        }

        public abstract IEnumerable<IDevice> GetCaptureDevices();
        public virtual Task<IEnumerable<IDevice>> GetCaptureDevicesAsync()
        {
            return Task.Factory.StartNew(() => GetCaptureDevices());
        }

        public abstract IEnumerable<IDevice> GetCaptureDevices(DeviceState deviceState);
        public virtual Task<IEnumerable<IDevice>> GetCaptureDevicesAsync(DeviceState deviceState)
        {
            return Task.Factory.StartNew(() => GetCaptureDevices(deviceState));
        }

        public abstract bool SetDefaultDevice(IDevice dev);
        public virtual Task<bool> SetDefaultDeviceAsync(IDevice dev)
        {
            return Task.Factory.StartNew(() => SetDefaultDevice(dev));
        }

        public abstract bool SetDefaultCommunicationsDevice(IDevice dev);
        public virtual Task<bool> SetDefaultCommunicationsDeviceAsync(IDevice dev)
        {
            return Task.Factory.StartNew(() => SetDefaultDevice(dev));
        }

        protected virtual void OnAudioDeviceChanged(object sender, DeviceChangedEventArgs e)
        {
            var handler = AudioDeviceChanged;

            //Bubble the event
            if (handler != null)
                handler(sender, e);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }

    }
}