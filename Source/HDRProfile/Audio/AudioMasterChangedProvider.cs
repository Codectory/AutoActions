using AudioSwitcher.AudioApi;
using System;

namespace AutoHDR.Audio
{
    class AudioMasterChangedProvider : IObserver<DeviceChangedArgs>
    {
        public event EventHandler<DeviceChangedArgs> DeviceChanged;

        public void OnCompleted()
        {
          //  throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
           // throw new NotImplementedException();
        }

        public void OnNext(DeviceChangedArgs value)
        {
            DeviceChanged?.Invoke(this, value);
        }
    }
}