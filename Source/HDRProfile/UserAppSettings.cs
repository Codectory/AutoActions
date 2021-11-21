using CodectoryCore.UI.Wpf;
using AutoHDR.Displays;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoHDR.Profiles;

namespace AutoHDR
{
    [DataContract]
    public class UserAppSettings : BaseViewModel
    {
        private bool _globalAutoHDR = true;
        private bool _logging = false;
        private bool _autoStart;
        private bool _startMinimizedToTray;
        private bool _closeToTray;
        private bool _checkForNewVersion = true;
        private HDRActivationMode _hdrMode;
        readonly object _audioDevicesLock = new object();
        private SortableObservableCollection<ApplicationProfileAssignment> _applicationProfileAssignments;
        private ObservableCollection<Profile> _applicationProfiles;
        private ObservableCollection<Display> _monitors;



        [DataMember]
        public bool GlobalAutoHDR { get => _globalAutoHDR; set { _globalAutoHDR = value; OnPropertyChanged(); } }

        [DataMember]
        public bool AutoStart { get => _autoStart; set { _autoStart = value; OnPropertyChanged(); } }

        [DataMember]
        public bool Logging { get => _logging; set { _logging = value; OnPropertyChanged(); } }

        [DataMember]
        public bool StartMinimizedToTray { get => _startMinimizedToTray; set { _startMinimizedToTray = value; OnPropertyChanged(); }  }

        [DataMember]
        public bool CloseToTray { get => _closeToTray; set { _closeToTray = value; OnPropertyChanged(); } }


        [DataMember]
        public bool CheckForNewVersion { get => _checkForNewVersion; set { _checkForNewVersion = value; OnPropertyChanged(); } }

        [DataMember]
        public HDRActivationMode HDRMode { get => _hdrMode; set { _hdrMode = value; OnPropertyChanged(); } }

        [DataMember]
        public SortableObservableCollection<ApplicationProfileAssignment> ApplicationProfileAssignments { get => _applicationProfileAssignments; set { _applicationProfileAssignments = value; OnPropertyChanged();} }

        [DataMember]

        public ObservableCollection<Profile> ApplicationProfiles { get => _applicationProfiles; set { _applicationProfiles = value; OnPropertyChanged(); } }


        [DataMember]
        public ObservableCollection<Display> Monitors { get => _monitors; set { _monitors = value; OnPropertyChanged(); } }


        public UserAppSettings()
        {
            ApplicationProfileAssignments = new SortableObservableCollection<ApplicationProfileAssignment>(new ObservableCollection<ApplicationProfileAssignment>());
            ApplicationProfiles = new ObservableCollection<Profile>();
            Monitors = new ObservableCollection<Display>();
        }

        public static UserAppSettings ReadSettings(string path)
        {
            UserAppSettings settings = null;
            XmlSerializer serializer = new XmlSerializer(typeof(UserAppSettings));
            using (TextReader reader = new StreamReader(path))
            {
                settings = (UserAppSettings)serializer.Deserialize(reader);
            }
            return settings;
        }

        public static UserAppSettings Convert(HDRProfileSettings settings)
        {
            UserAppSettings convertedSettings = new UserAppSettings();
            convertedSettings.AutoStart = settings.AutoStart;
            convertedSettings.CheckForNewVersion = settings.CheckForNewVersion;
            convertedSettings.CloseToTray = settings.CloseToTray;
            convertedSettings.GlobalAutoHDR = settings.GlobalAutoHDR;
            convertedSettings.HDRMode = settings.HDRMode;
            convertedSettings.Logging = settings.Logging;
            convertedSettings.Monitors = settings.Monitors;
            convertedSettings.StartMinimizedToTray = settings.StartMinimizedToTray;
            return convertedSettings;
        }
    }

    public static class UserAppSettingsExtension
    {

        public static void SaveSettings(this UserAppSettings setting, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserAppSettings));
            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, setting);
            }
        }

    }

}
