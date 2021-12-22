using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Audio
{
    public class AudioDevice
    {

        CoreAudioDevice BaseDevice;
        public AudioDeviceType DeviceType
        {
            get
            {
                switch (BaseDevice.DeviceType)
                {
                    case AudioSwitcher.AudioApi.DeviceType.All:
                        return AudioDeviceType.All;
                    case AudioSwitcher.AudioApi.DeviceType.Capture:
                        return AudioDeviceType.Capture;
                    case AudioSwitcher.AudioApi.DeviceType.Playback:
                        return AudioDeviceType.Playback;
                    default:
                        return AudioDeviceType.Unknown;
                }
            }
        }
        public  DeviceState State 
        {
            get
            {
                switch(BaseDevice.State)
                {
                    case AudioSwitcher.AudioApi.DeviceState.All:
                        return DeviceState.All;
                    case AudioSwitcher.AudioApi.DeviceState.Active:
                        return DeviceState.Active;
                    case AudioSwitcher.AudioApi.DeviceState.NotPresent:
                        return DeviceState.NotPresent;
                    case AudioSwitcher.AudioApi.DeviceState.Unplugged:
                        return DeviceState.Unplugged;
                    default:
                        return DeviceState.Disabled;
                }
            }
        }

        public bool IsDefaultCommunicationsDevice => BaseDevice.IsDefaultCommunicationsDevice;
        public bool IsDefaultDevice => BaseDevice.IsDefaultDevice;
        public string Name  => BaseDevice.FullName;
        public Guid ID  => BaseDevice.Id;

        public bool IsMuted => BaseDevice.IsMuted;
        public double Volume => BaseDevice.Volume;



        public AudioDevice(CoreAudioDevice coreAudioDevice)
        {
            BaseDevice = coreAudioDevice;
        }
        public void SetAsDefault()
        {
            BaseDevice.SetAsDefault();

        }

        public void SetAsDefaultCommunications()
        {
            BaseDevice.SetAsDefaultCommunications();

        }

        public void SetMute(bool mute)
        {
            BaseDevice.Mute(mute);
        }
        public void SetVolume(int volume)
        {
            BaseDevice.Volume = volume;
        }





    }
}
