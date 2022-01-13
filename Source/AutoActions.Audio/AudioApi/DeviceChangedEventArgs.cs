using System;

namespace AudioSwitcher.AudioApi
{

    public abstract class DeviceChangedEventArgs : EventArgs
    {

        public IDevice Device { get; private set; }

        public AudioDeviceEventType EventType { get; private set; }

        protected DeviceChangedEventArgs(IDevice dev, AudioDeviceEventType type)
        {
            Device = dev;
            EventType = type;
        }

    }
}