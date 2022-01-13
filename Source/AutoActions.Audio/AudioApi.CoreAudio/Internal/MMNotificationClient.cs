using System;
using AudioSwitcher.AudioApi.CoreAudio.Interfaces;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    internal sealed class MMNotificationClient : IMMNotificationClient
    {
        private readonly WeakReference _innerClient;

        internal MMNotificationClient(ISystemAudioEventClient innerClient)
        {
            _innerClient = new WeakReference(innerClient);
        }

        public void OnDeviceStateChanged(string deviceId, EDeviceState newState)
        {
            var inner = GetInner();
            if(inner != null)
                inner.OnDeviceStateChanged(deviceId, newState);
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            var inner = GetInner();
            if(inner != null)
                inner.OnDeviceAdded(pwstrDeviceId);
        }

        public void OnDeviceRemoved(string deviceId)
        {
            var inner = GetInner();
            if(inner != null)
                inner.OnDeviceRemoved(deviceId);
        }

        public void OnDefaultDeviceChanged(EDataFlow flow, ERole role, string defaultDeviceId)
        {
            var inner = GetInner();
            if(inner != null)
                inner.OnDefaultDeviceChanged(flow, role, defaultDeviceId);
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            var inner = GetInner();
            if(inner != null)
                inner.OnPropertyValueChanged(pwstrDeviceId, key);
        }

        private ISystemAudioEventClient GetInner()
        {
            if (_innerClient.IsAlive)
                return _innerClient.Target as ISystemAudioEventClient;

            return null;
        }
    }
}
