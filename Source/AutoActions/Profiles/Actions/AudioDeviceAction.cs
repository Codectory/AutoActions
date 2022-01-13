using AutoActions.Audio;
using AutoActions.ProjectResources;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace AutoActions.Profiles.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AudioDeviceAction : ProfileActionBase
    {

        public override bool CanSave => (ChangePlaybackDevice && PlaybackDevice!=null) || (ChangeRecordDevice && RecordDevice != null);
        public override string CannotSaveMessage => ProjectLocales.MessageMissingAudioDevice;

        public override string ActionTypeName => ProjectLocales.AudioAction;


        private Guid _playbackDeviceID = Guid.Empty;

        [JsonProperty]
        public Guid PlaybackDeviceID { get => _playbackDeviceID; set { _playbackDeviceID = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlaybackDevice)); } }


        public AudioDevice PlaybackDevice { get => AudioController.Instance.OutputAudioDevices.FirstOrDefault(d => d.ID.Equals(PlaybackDeviceID)); set { PlaybackDeviceID = value.ID; } }


        private bool _setOutput = false;

        [JsonProperty]
        public bool ChangePlaybackDevice { get => _setOutput; set { _setOutput = value; OnPropertyChanged(); } }


        private Guid _recodDeviceID =Guid.Empty;

        [JsonProperty]
        public Guid RecordDeviceID { get => _recodDeviceID; set { _recodDeviceID = value; OnPropertyChanged(); OnPropertyChanged(nameof(RecordDevice)); } }



        public AudioDevice RecordDevice { get => AudioController.Instance.InputAudioDevices.FirstOrDefault(d => d.ID.Equals(RecordDeviceID)); set { RecordDeviceID = value.ID; } }

        private bool _setInput = false;

        [JsonProperty]
        public bool ChangeRecordDevice { get => _setInput; set { _setInput = value; OnPropertyChanged(); } }


        public override string ActionDescription => $"[{(PlaybackDevice != null && ChangePlaybackDevice ?  PlaybackDevice.Name : string.Empty)} {(RecordDevice != null && ChangeRecordDevice ? RecordDevice.Name : string.Empty)}]";


        public AudioDeviceAction() : base()
        {
        }

        public override ActionEndResult RunAction(ApplicationChangedType applicationChangedType)
        {
            try
            {
                if (ChangePlaybackDevice)
                {
                    CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting playback device to {PlaybackDevice.Name}"));
                    PlaybackDevice.SetAsDefault();
                }
                if (ChangeRecordDevice)
                {
                    CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting record audio device to {RecordDevice.Name}"));
                    RecordDevice.SetAsDefault();
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
