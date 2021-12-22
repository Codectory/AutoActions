using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using AudioSwitcher.AudioApi.CoreAudio.Interfaces;
using AudioSwitcher.AudioApi.CoreAudio.Threading;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    public sealed partial class CoreAudioDevice : Device, INotifyPropertyChanged, IDisposable
    {
        private IMMDevice _device;
        private Guid? _id;
        private CachedPropertyDictionary _properties;
        private EDeviceState _state;
        private string _realId;
        private EDataFlow _dataFlow;

        internal CoreAudioDevice(IMMDevice device, IAudioController<CoreAudioDevice> controller)
            : base(controller)
        {
            ComThread.Assert();

            _device = device;

            if (device == null)
                throw new ArgumentNullException("device");

            LoadProperties(device);

            ReloadAudioMeterInformation(device);
            ReloadAudioEndpointVolume(device);

            controller.AudioDeviceChanged +=
                new EventHandler<DeviceChangedEventArgs>(EnumeratorOnAudioDeviceChanged)
                    .MakeWeak(x =>
                    {
                        controller.AudioDeviceChanged -= x;
                    });
        }

        private void LoadProperties(IMMDevice device)
        {
            ComThread.Assert();

            //Load values
            Marshal.ThrowExceptionForHR(device.GetId(out _realId));
            Marshal.ThrowExceptionForHR(device.GetState(out _state));

            // ReSharper disable once SuspiciousTypeConversion.Global
            var ep = device as IMMEndpoint;
            if (ep != null)
                ep.GetDataFlow(out _dataFlow);

            GetPropertyInformation(device);
        }

        ~CoreAudioDevice()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            ClearAudioEndpointVolume();
            ClearAudioMeterInformation();

            _device = null;
        }

        /// <summary>
        ///     Unique identifier for this device
        /// </summary>
        public override Guid Id
        {
            get
            {
                if (_id == null)
                    _id = SystemIdToGuid(RealId);

                return _id.Value;
            }
        }

        public string RealId
        {
            get
            {
                return _realId;
            }
        }

        public override string InterfaceName
        {
            get
            {
                if (Properties != null && Properties.Contains(PropertyKeys.PKEY_DEVICE_INTERFACE_FRIENDLY_NAME))
                    return Properties[PropertyKeys.PKEY_DEVICE_INTERFACE_FRIENDLY_NAME] as string;
                return "Unknown";
            }
        }

        /// <summary>
        /// The short name e.g. Speaker/Headphones etc..
        /// </summary>
        public override string Name
        {
            get
            {
                if (Properties != null && Properties.Contains(PropertyKeys.PKEY_DEVICE_DESCRIPTION))
                    return Properties[PropertyKeys.PKEY_DEVICE_DESCRIPTION] as string;

                return InterfaceName;
            }
            set
            {
                if (Properties != null && Properties.Contains(PropertyKeys.PKEY_DEVICE_DESCRIPTION))
                    Properties[PropertyKeys.PKEY_DEVICE_DESCRIPTION] = value;
            }
        }

        public override string FullName
        {
            get
            {
                if (Properties != null && Properties.Contains(PropertyKeys.PKEY_DEVICE_FRIENDLY_NAME))
                    return Properties[PropertyKeys.PKEY_DEVICE_FRIENDLY_NAME] as string;
                return "Unknown";
            }
        }

        public override DeviceIcon Icon
        {
            get
            {
                if (Properties != null && Properties.Contains(PropertyKeys.PKEY_DEVICE_ICON))
                    return IconStringToDeviceIcon(Properties[PropertyKeys.PKEY_DEVICE_ICON] as string);

                return DeviceIcon.Unknown;
            }
        }

        public override bool IsDefaultDevice
        {
            get
            {
                IDevice defaultDevice = null;

                if (IsPlaybackDevice)
                    defaultDevice = Controller.DefaultPlaybackDevice;
                else if (IsCaptureDevice)
                    defaultDevice = Controller.DefaultCaptureDevice;

                return defaultDevice != null && defaultDevice.Id == Id;
            }
        }

        public override bool IsDefaultCommunicationsDevice
        {
            get
            {
                IDevice defaultDevice = null;

                if (IsPlaybackDevice)
                    defaultDevice = Controller.DefaultPlaybackCommunicationsDevice;
                else if (IsCaptureDevice)
                    defaultDevice = Controller.DefaultCaptureCommunicationsDevice;

                return defaultDevice != null && defaultDevice.Id == Id;
            }
        }

        public override DeviceState State
        {
            get { return _state.AsDeviceState(); }
        }

        public override DeviceType DeviceType
        {
            get { return _dataFlow.AsDeviceType(); }
        }

        public override bool IsMuted
        {
            get
            {
                if (AudioEndpointVolume == null)
                    return false;

                return AudioEndpointVolume.Mute;
            }
        }

        /// <summary>
        ///     The volume level on a scale between 0-100. Returns -1 if end point does not have volume
        /// </summary>
        public override int Volume
        {
            get
            {
                if (AudioEndpointVolume == null)
                    return -1;

                return (int)Math.Round(AudioEndpointVolume.MasterVolumeLevelScalar * 100, 0);
            }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > 100)
                    value = 100;

                float val = (float)value / 100;

                if (AudioEndpointVolume == null)
                    return;

                AudioEndpointVolume.MasterVolumeLevelScalar = val;

                //Something is up with the floating point numbers in Windows, so make sure the volume is correct
                if (AudioEndpointVolume.MasterVolumeLevelScalar < val)
                    AudioEndpointVolume.MasterVolumeLevelScalar += 0.0001F;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void EnumeratorOnAudioDeviceChanged(object sender, DeviceChangedEventArgs deviceChangedEventArgs)
        {
            if (deviceChangedEventArgs.Device == null || deviceChangedEventArgs.Device.Id != Id)
                return;

            var propertyChangedEvent = deviceChangedEventArgs as DevicePropertyChangedEventArgs;
            var stateChangedEvent = deviceChangedEventArgs as DeviceStateChangedEventArgs;
            var defaultChangedEvent = deviceChangedEventArgs as DefaultDeviceChangedEventArgs;

            if (propertyChangedEvent != null)
                HandlePropertyChanged(propertyChangedEvent);

            if (stateChangedEvent != null)
                HandleStateChanged(stateChangedEvent);

            if (defaultChangedEvent != null)
                HandleDefaultChanged(defaultChangedEvent);
        }

        private void HandlePropertyChanged(DevicePropertyChangedEventArgs propertyChangedEvent)
        {
            ComThread.BeginInvoke(() =>
            {
                LoadProperties(_device);
            })
            .ContinueWith(x =>
            {
                OnPropertyChanged(propertyChangedEvent.PropertyName);
            });
        }

        private void HandleStateChanged(DeviceStateChangedEventArgs stateChangedEvent)
        {
            _state = stateChangedEvent.State.AsEDeviceState();

            ReloadAudioEndpointVolume(_device);
            ReloadAudioMeterInformation(_device);

            OnPropertyChanged("State");
        }

        private void ReloadAudioMeterInformation(IMMDevice device)
        {
            ComThread.BeginInvoke(() =>
            {
                LoadAudioMeterInformation(device);
            });
        }

        private void ReloadAudioEndpointVolume(IMMDevice device)
        {
            ComThread.BeginInvoke(() =>
            {
                LoadAudioEndpointVolume(device);

                if (AudioEndpointVolume != null)
                    AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
            });
        }

        private void HandleDefaultChanged(DefaultDeviceChangedEventArgs defaultChangedEvent)
        {
            if (defaultChangedEvent.IsDefaultEvent)
                OnPropertyChanged("IsDefaultDevice");
            else if (defaultChangedEvent.IsDefaultCommunicationsEvent)
                OnPropertyChanged("IsDefaultCommunicationsDevice");
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            RaiseVolumeChanged(Volume);

            //Fire the muted changed here too
            OnPropertyChanged("IsMuted");
        }

        private void RaiseVolumeChanged(int newVolume)
        {
            var handler = VolumeChanged;

            if (handler != null)
                handler(this, new DeviceVolumeChangedEventArgs(this, newVolume));

            OnPropertyChanged("Volume");
        }

        public override bool Mute(bool mute)
        {
            if (AudioEndpointVolume == null)
                return false;

            AudioEndpointVolume.Mute = mute;
            return AudioEndpointVolume.Mute;
        }

        public override event EventHandler<DeviceChangedEventArgs> VolumeChanged;

        /// <summary>
        ///     Extracts the unique GUID Identifier for a Windows System _device
        /// </summary>
        /// <param name="systemDeviceId"></param>
        /// <returns></returns>
        public static Guid SystemIdToGuid(string systemDeviceId)
        {
            return systemDeviceId.ExtractGuids().First();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}