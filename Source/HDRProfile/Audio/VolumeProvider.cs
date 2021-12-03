using AudioSwitcher.AudioApi;
using System;

namespace AutoHDR.Audio
{
    internal class VolumeProvider : IObserver<DeviceVolumeChangedArgs>
    {
        public bool Paused = false;

        public event EventHandler<DeviceVolumeChangedArgs> VolumeChanged;

        public void OnCompleted()
        {
           // throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            //throw new NotImplementedException();
        }

        public void OnNext(DeviceVolumeChangedArgs value)
        {
            if (!Paused)
                VolumeChanged?.Invoke(this, value);
        }
    }
}