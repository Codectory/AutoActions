using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoHDR.Audio
{
    public class AudioController
    {
        private static AudioController _instance = null;
        public static AudioController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AudioController();
                return _instance;
            }
        }
        internal CoreAudioController Controller { get; private set; } 

        static readonly object _lockDevices = new object();
        static readonly object _lockDevicesChanged= new object();


        List<AudioDevice> _outputAudioDevices = new List<AudioDevice>();

        List<AudioDevice> _inputAudioDevices = new List<AudioDevice>();



        public IReadOnlyList<AudioDevice> OutputAudioDevices { get { lock (_lockDevices) { return  _outputAudioDevices.AsReadOnly(); } } }
        public IReadOnlyList<AudioDevice> InputAudioDevices {  get { lock (_lockDevices) { return _inputAudioDevices.AsReadOnly(); } } }

        public AudioController()
        {
            Controller = new CoreAudioController();
            Controller.AudioDeviceChanged += Controller_AudioDeviceChanged;
            UpdateDevices();

        }

        private void Controller_AudioDeviceChanged(object sender, AudioSwitcher.AudioApi.DeviceChangedEventArgs e)
        {
            UpdateDevices();
        }

        public void UpdateDevices()
        {
            lock (_lockDevices)
            {
                List<CoreAudioDevice> devices = Controller.GetDevices(AudioSwitcher.AudioApi.DeviceState.All).ToList();

                List<AudioDevice> outputAudioDevices = new List<AudioDevice>();
                List<AudioDevice> inputAudioDevices = new List<AudioDevice>();

                for (int i = 0; i < devices.Count(); i++)
                {
                    CoreAudioDevice baseDevice = devices[i];
                    AudioDevice device = new AudioDevice(baseDevice);
                    if (device.DeviceType == AudioDeviceType.Capture && !inputAudioDevices.Any(d => d.ID == device.ID))
                        inputAudioDevices.Add(device);

                    if (device.DeviceType == AudioDeviceType.Playback && !outputAudioDevices.Any(d => d.ID == device.ID))
                        outputAudioDevices.Add(device);

                }

                List<AudioDevice> outputsToDelete = new List<AudioDevice>();
                List<AudioDevice> inputsToDelete = new List<AudioDevice>();

                foreach (var oldDevice in _outputAudioDevices)
                    if (!outputAudioDevices.Any(d => d.ID == oldDevice.ID))
                        outputsToDelete.Add(oldDevice);

                foreach (var oldDevice in _inputAudioDevices)
                    if (!inputAudioDevices.Any(d => d.ID == oldDevice.ID))
                        inputsToDelete.Add(oldDevice);

                foreach (var oldDevice in outputsToDelete)
                    _outputAudioDevices.Remove(oldDevice);
                foreach (var oldDevice in inputsToDelete)
                    _inputAudioDevices.Remove(oldDevice);


                foreach (var newDevice in outputAudioDevices)
                    if (!_outputAudioDevices.Any(d => d.ID == newDevice.ID))
                        _outputAudioDevices.Add(newDevice);

                foreach (var newDevice in inputAudioDevices)
                    if (!_inputAudioDevices.Any(d => d.ID == newDevice.ID))
                        _inputAudioDevices.Add(newDevice);
            }
        }
    }
}
