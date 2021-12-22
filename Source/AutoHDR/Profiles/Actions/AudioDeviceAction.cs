using AutoHDR.Audio;
using AutoHDR.ProjectResources;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace AutoHDR.Profiles.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AudioDeviceAction : ProfileActionBase
    {
        public override string ActionTypeName => ProjectLocales.AudioAction;



        private Guid _outputDeviceID = Guid.Empty;

        [JsonProperty]
        public Guid OutputDeviceID { get => _outputDeviceID; set { _outputDeviceID = value; OnPropertyChanged(); OnPropertyChanged(nameof(OutputDevice)); } }


        public AudioDevice OutputDevice { get => AudioController.Instance.OutputAudioDevices.FirstOrDefault(d => d.ID.Equals(OutputDeviceID)); set { OutputDeviceID = value.ID; } }


        private bool _setOutput = false;

        [JsonProperty]
        public bool SetOutput { get => _setOutput; set { _setOutput = value; OnPropertyChanged(); } }


        private Guid _inputDeviceID =Guid.Empty;

        [JsonProperty]
        public Guid InputDeviceID { get => _inputDeviceID; set { _inputDeviceID = value; OnPropertyChanged(); OnPropertyChanged(nameof(InputDevice)); } }



        public AudioDevice InputDevice { get => AudioController.Instance.InputAudioDevices.FirstOrDefault(d => d.ID.Equals(InputDeviceID)); set { InputDeviceID = value.ID; } }

        private bool _setInput = false;

        [JsonProperty]
        public bool SetInput { get => _setInput; set { _setInput = value; OnPropertyChanged(); } }


        public override string ActionDescription => $"[{(OutputDevice != null && SetOutput ?  OutputDevice.Name : string.Empty)} {(InputDevice != null && SetInput ? InputDevice.Name : string.Empty)}]";


        public AudioDeviceAction() : base()
        {
        }

        public override ActionEndResult RunAction(params object[] parameter)
        {
            try
            {
                if (SetOutput)
                {
                    CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting output audio device to {OutputDevice.Name}"));
                    OutputDevice.SetAsDefault();
                }
                if (SetInput)
                {
                    CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting input audio device to {InputDevice.Name}"));
                    InputDevice.SetAsDefault();
                }
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                return new ActionEndResult(false, ex.Message, ex);
            }
        }
    }
}
