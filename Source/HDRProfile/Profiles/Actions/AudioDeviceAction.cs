using AudioSwitcher.AudioApi.CoreAudio;
using AutoHDR.Audio;
using AutoHDR.ProjectResources;
using CodectoryCore.UI.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Profiles.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AudioDeviceAction : ProfileActionBase
    {
        public override string ActionTypeName => ProjectResources.Locale_Texts.AudioAction;



        private Guid _outputDeviceGuid = Guid.Empty;

        [JsonProperty]
        public Guid OutputDeviceGuid { get => _outputDeviceGuid; set { _outputDeviceGuid = value; OnPropertyChanged(); OnPropertyChanged(nameof(OutputDevice)); } }


        private CoreAudioDevice _outputDevice = null;

        public CoreAudioDevice OutputDevice { get => AudioManager.OutputAudioDevices.FirstOrDefault(d => d.Id.Equals(OutputDeviceGuid)); set { OutputDeviceGuid = value.Id; } }


        private bool _setOutput = false;

        [JsonProperty]
        public bool SetOutput { get => _setOutput; set { _setOutput = value; OnPropertyChanged(); } }


        private Guid _inputDeviceGuid = Guid.Empty;

        [JsonProperty]
        public Guid InputDeviceGuid { get => _inputDeviceGuid; set { _inputDeviceGuid = value; OnPropertyChanged(); OnPropertyChanged(nameof(InputDevice)); } }



        public CoreAudioDevice InputDevice { get => AudioManager.InputAudioDevices.FirstOrDefault(d => d.Id.Equals(InputDeviceGuid)); set { InputDeviceGuid = value.Id; } }

        private bool _setInput = false;

        [JsonProperty]
        public bool SetInput { get => _setInput; set { _setInput = value; OnPropertyChanged(); } }


        public override string ActionDescription => $"[{(OutputDevice != null && SetOutput ?  OutputDevice.FullName : string.Empty)} {(InputDevice != null && SetInput ? InputDevice.FullName : string.Empty)}]";


        public AudioDeviceAction()
        {
        }

        public override ActionEndResult RunAction(params object[] parameter)
        {
            try
            {
                if (SetOutput)
                {
                    CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting output audio device to {OutputDevice.FullName}"));
                    OutputDevice.SetAsDefault();
                }
                if (SetInput)
                {
                    CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting input audio device to {InputDevice.FullName}"));
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
