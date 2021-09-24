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
        private OnOff _desktopHDRDefault = OnOff.OFF;
        readonly object _audioDevicesLock = new object();
        private ObservableCollection<ApplicationItem> _applicationItems;
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
        public OnOff DesktopHDRDefault { get => _desktopHDRDefault; set { _desktopHDRDefault = value; OnPropertyChanged(); } }
        [DataMember]
        public ObservableCollection<ApplicationItem> ApplicationItems { get => _applicationItems; set {_applicationItems = value; OnPropertyChanged();} }


        [DataMember]
        public ObservableCollection<Display> Monitors { get => _monitors; set { _monitors = value; OnPropertyChanged(); } }


        public UserAppSettings()
        {
            ApplicationItems = new ObservableCollection<ApplicationItem>();
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
            convertedSettings.ApplicationItems = settings.ApplicationItems;
            convertedSettings.AutoStart = settings.AutoStart;
            convertedSettings.CheckForNewVersion = settings.CheckForNewVersion;
            convertedSettings.CloseToTray = settings.CloseToTray;
            convertedSettings.GlobalAutoHDR = settings.GlobalAutoHDR;
            convertedSettings.HDRMode = settings.HDRMode;
            convertedSettings.DesktopHDRDefault = settings.DesktopHDRDefault;
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
