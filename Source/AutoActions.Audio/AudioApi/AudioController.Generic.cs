using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AudioSwitcher.AudioApi
{
    public abstract class AudioController<T> : IAudioController<T>
        where T : class, IDevice
    {
        protected const DeviceState DEFAULT_DEVICE_STATE_FILTER =
            DeviceState.Active | DeviceState.Unplugged | DeviceState.Disabled;

        public event EventHandler<DeviceChangedEventArgs> AudioDeviceChanged;

        protected virtual void OnAudioDeviceChanged(object sender, DeviceChangedEventArgs e)
        {
            var handler = AudioDeviceChanged;

            //Bubble the event
            if (handler != null)
                handler(sender, e);
        }

        public T DefaultPlaybackDevice
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

        public T DefaultPlaybackCommunicationsDevice
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

        public T DefaultCaptureDevice
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

        public T DefaultCaptureCommunicationsDevice
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

        public virtual T GetDevice(Guid id)
        {
            return GetDevice(id, DEFAULT_DEVICE_STATE_FILTER);
        }

        public virtual Task<T> GetDeviceAsync(Guid id)
        {
            return Task.Factory.StartNew(() => GetDevice(id));
        }

        public abstract T GetDevice(Guid id, DeviceState state);
        public virtual Task<T> GetDeviceAsync(Guid id, DeviceState state)
        {
            return Task.Factory.StartNew(() => GetDevice(id, state));
        }

        public abstract T GetDefaultDevice(DeviceType deviceType, Role role);
        public virtual Task<T> GetDefaultDeviceAsync(DeviceType deviceType, Role role)
        {
            return Task.Factory.StartNew(() => GetDefaultDevice(deviceType, role));
        }

        public virtual IEnumerable<T> GetDevices()
        {
            return GetDevices(DEFAULT_DEVICE_STATE_FILTER);
        }

        public virtual Task<IEnumerable<T>> GetDevicesAsync()
        {
            return Task.Factory.StartNew(() => GetDevices());
        }

        public virtual Task<IEnumerable<T>> GetDevicesAsync(DeviceState state)
        {
            return Task.Factory.StartNew(() => GetDevices(state));
        }

        public IEnumerable<T> GetDevices(DeviceType deviceType)
        {
            return GetDevices(deviceType, DEFAULT_DEVICE_STATE_FILTER);
        }

        public Task<IEnumerable<T>> GetDevicesAsync(DeviceType deviceType)
        {
            return GetDevicesAsync(deviceType, DEFAULT_DEVICE_STATE_FILTER);
        }

        IEnumerable<IDevice> IAudioController.GetDevices(DeviceType deviceType)
        {
            return GetDevices(deviceType);
        }

        Task<IEnumerable<IDevice>> IAudioController.GetDevicesAsync(DeviceType deviceType)
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.Factory.StartNew(() => GetDevices(deviceType).Cast<IDevice>());
        }

        public virtual IEnumerable<T> GetDevices(DeviceState state)
        {
            return GetDevices(DeviceType.All, state);
        }

        public abstract IEnumerable<T> GetDevices(DeviceType deviceType, DeviceState state);

        public virtual Task<IEnumerable<T>> GetDevicesAsync(DeviceType deviceType, DeviceState state)
        {
            return Task.Factory.StartNew(() => GetDevices(deviceType, state));
        }

        public virtual IEnumerable<T> GetPlaybackDevices()
        {
            return GetPlaybackDevices(DEFAULT_DEVICE_STATE_FILTER);
        }

        public virtual IEnumerable<T> GetPlaybackDevices(DeviceState state)
        {
            return GetDevices(DeviceType.Playback, state);
        }

        public virtual Task<IEnumerable<T>> GetPlaybackDevicesAsync()
        {
            return GetPlaybackDevicesAsync(DEFAULT_DEVICE_STATE_FILTER);
        }

        public virtual Task<IEnumerable<T>> GetPlaybackDevicesAsync(DeviceState deviceState)
        {
            return Task.Factory.StartNew(() => GetPlaybackDevices(deviceState));
        }

        public virtual IEnumerable<T> GetCaptureDevices()
        {
            return GetCaptureDevices(DEFAULT_DEVICE_STATE_FILTER);
        }

        public virtual Task<IEnumerable<T>> GetCaptureDevicesAsync()
        {
            return Task.Factory.StartNew(() => GetCaptureDevices(DEFAULT_DEVICE_STATE_FILTER));
        }

        public virtual IEnumerable<T> GetCaptureDevices(DeviceState state)
        {
            return GetDevices(DeviceType.Capture, state);
        }

        public virtual Task<IEnumerable<T>> GetCaptureDevicesAsync(DeviceState deviceState)
        {
            return Task.Factory.StartNew(() => GetCaptureDevices(deviceState));
        }

        public abstract bool SetDefaultDevice(T dev);

        public virtual Task<bool> SetDefaultDeviceAsync(T dev)
        {
            return Task.Factory.StartNew(() => SetDefaultDevice(dev));
        }

        public abstract bool SetDefaultCommunicationsDevice(T dev);

        public virtual Task<bool> SetDefaultCommunicationsDeviceAsync(T dev)
        {
            return Task.Factory.StartNew(() => SetDefaultCommunicationsDevice(dev));
        }

        Task<IDevice> IAudioController.GetDeviceAsync(Guid id)
        {
            return Task.Factory.StartNew(() => GetDevice(id) as IDevice);
        }

        IDevice IAudioController.DefaultPlaybackDevice
        {
            get { return DefaultPlaybackDevice; }
            set { SetDefaultDevice(value); }
        }

        IDevice IAudioController.DefaultPlaybackCommunicationsDevice
        {
            get { return DefaultPlaybackCommunicationsDevice; }
            set { SetDefaultCommunicationsDevice(value); }
        }

        IDevice IAudioController.DefaultCaptureDevice
        {
            get { return DefaultCaptureDevice; }
            set { SetDefaultDevice(value); }
        }

        IDevice IAudioController.DefaultCaptureCommunicationsDevice
        {
            get { return DefaultCaptureCommunicationsDevice; }
            set { SetDefaultCommunicationsDevice(value); }
        }


        IDevice IAudioController.GetDevice(Guid id)
        {
            return GetDevice(id);
        }

        IDevice IAudioController.GetDevice(Guid id, DeviceState state)
        {
            return GetDevice(id, state);
        }

        Task<IDevice> IAudioController.GetDeviceAsync(Guid id, DeviceState state)
        {
            return Task.Factory.StartNew(() => GetDevice(id, state) as IDevice);
        }

        IDevice IAudioController.GetDefaultDevice(DeviceType deviceType, Role role)
        {
            return GetDefaultDevice(deviceType, role);
        }

        Task<IDevice> IAudioController.GetDefaultDeviceAsync(DeviceType deviceType, Role role)
        {
            return Task.Factory.StartNew(() => GetDefaultDevice(deviceType, role) as IDevice);
        }

        IEnumerable<IDevice> IAudioController.GetDevices()
        {
            return GetDevices();
        }

        Task<IEnumerable<IDevice>> IAudioController.GetDevicesAsync()
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.Factory.StartNew(() => GetDevices().Cast<IDevice>());
        }

        IEnumerable<IDevice> IAudioController.GetDevices(DeviceState state)
        {
            return GetDevices(state);
        }

        Task<IEnumerable<IDevice>> IAudioController.GetDevicesAsync(DeviceState state)
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.Factory.StartNew(() => GetDevices(state).Cast<IDevice>());
        }

        IEnumerable<IDevice> IAudioController.GetDevices(DeviceType deviceType, DeviceState state)
        {
            return GetDevices(deviceType, state);
        }

        Task<IEnumerable<IDevice>> IAudioController.GetDevicesAsync(DeviceType deviceType, DeviceState state)
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.Factory.StartNew(() => GetDevices(deviceType, state).Cast<IDevice>());
        }

        IEnumerable<IDevice> IAudioController.GetPlaybackDevices()
        {
            return GetPlaybackDevices();
        }

        IEnumerable<IDevice> IAudioController.GetPlaybackDevices(DeviceState state)
        {
            return GetPlaybackDevices(state);
        }

        Task<IEnumerable<IDevice>> IAudioController.GetPlaybackDevicesAsync()
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.Factory.StartNew(() => GetPlaybackDevices().Cast<IDevice>());
        }

        Task<IEnumerable<IDevice>> IAudioController.GetPlaybackDevicesAsync(DeviceState deviceState)
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.Factory.StartNew(() => GetPlaybackDevices(deviceState).Cast<IDevice>());
        }

        IEnumerable<IDevice> IAudioController.GetCaptureDevices()
        {
            return GetCaptureDevices();
        }

        IEnumerable<IDevice> IAudioController.GetCaptureDevices(DeviceState state)
        {
            return GetCaptureDevices(state);
        }

        Task<IEnumerable<IDevice>> IAudioController.GetCaptureDevicesAsync()
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.Factory.StartNew(() => GetCaptureDevices().Cast<IDevice>());
        }

        Task<IEnumerable<IDevice>> IAudioController.GetCaptureDevicesAsync(DeviceState deviceState)
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return Task.Factory.StartNew(() => GetCaptureDevices(deviceState).Cast<IDevice>());
        }

        public virtual bool SetDefaultDevice(IDevice dev)
        {
            var device = dev as T;
            if (device != null)
                return SetDefaultDevice(device);

            return false;
        }

        public virtual Task<bool> SetDefaultDeviceAsync(IDevice dev)
        {
            return Task.Factory.StartNew(() => SetDefaultDevice(dev));
        }

        public virtual bool SetDefaultCommunicationsDevice(IDevice dev)
        {
            var device = dev as T;
            if (device != null)
                return SetDefaultCommunicationsDevice(device);

            return false;
        }

        public virtual Task<bool> SetDefaultCommunicationsDeviceAsync(IDevice dev)
        {
            return Task.Factory.StartNew(() => SetDefaultCommunicationsDevice(dev));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) { }

    }
}