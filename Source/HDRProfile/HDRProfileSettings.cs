using CodectoryCore.UI.Wpf;
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

namespace HDRProfile
{
    [DataContract]
    public class HDRProfileSettings : BaseViewModel
    {

        private bool _globalAutoHDR = true;
        private bool _logging = false;
        private bool _autoStart;
        private bool _startMinimizedToTray;
        private bool _closeToTray;
        private HDRActivationMode _hdrMode;
        readonly object _audioDevicesLock = new object();
        private ObservableCollection<ApplicationItem> _applicationItems;
        private ObservableCollection<Monitor> _monitors;



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
        public HDRActivationMode HDRMode { get => _hdrMode; set { _hdrMode = value; OnPropertyChanged(); } }

        [DataMember]
        public ObservableCollection<ApplicationItem> ApplicationItems { get => _applicationItems; set {_applicationItems = value; OnPropertyChanged();} }


        [DataMember]
        public ObservableCollection<Monitor> Monitors { get => _monitors; set { _monitors = value; OnPropertyChanged(); } }


        public HDRProfileSettings()
        {
            ApplicationItems = new ObservableCollection<ApplicationItem>();
            Monitors = new ObservableCollection<Monitor>();
        }


        public static HDRProfileSettings ReadSettings(string path)
        {
            HDRProfileSettings settings = null;
            XmlSerializer serializer = new XmlSerializer(typeof(HDRProfileSettings));
            using (TextReader reader = new StreamReader(path))
            {
                settings = (HDRProfileSettings)serializer.Deserialize(reader);
            }
            return settings;
        }
    }

    public static class HDRProfileHandlerExtension
    {

        public static void SaveSettings(this HDRProfileSettings setting, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(HDRProfileSettings));
            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, setting);
            }
        }

    }

}
