using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioSwitcher.AudioApi
{
    public sealed class DefaultDeviceChangedEventArgs : DeviceChangedEventArgs
    {

        public bool IsDefaultEvent { get; set; }
        public bool IsDefaultCommunicationsEvent { get; set; }

        public DefaultDeviceChangedEventArgs(IDevice dev, bool isCommunications = false)
            : base(dev, isCommunications ? AudioDeviceEventType.DefaultCommunicationsDevice : AudioDeviceEventType.DefaultDevice)
        {
            IsDefaultEvent = !isCommunications;
            IsDefaultCommunicationsEvent = isCommunications;
        }

    }
}
