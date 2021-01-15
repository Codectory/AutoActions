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

        private bool logging = false;
        private bool autoStart;
        private bool startMinimizedToTray;
        private bool _closeToTray;
        private HDRMode hdrMode;
        readonly object _audioDevicesLock = new object();
        private ObservableCollection<ApplicationItem> _applicationItems;



        [DataMember]
        public bool AutoStart { get => false/*autoStart*/; set { autoStart = value; OnPropertyChanged(); } }

        [DataMember]
        public bool Logging { get => logging; set { logging = value; OnPropertyChanged(); } }

        [DataMember]
        public bool StartMinimizedToTray { get => startMinimizedToTray; set { startMinimizedToTray = value; OnPropertyChanged(); }  }

        [DataMember]
        public bool CloseToTray { get => _closeToTray; set { _closeToTray = value; OnPropertyChanged(); } }


        [DataMember]
        public HDRMode HDRMode { get => hdrMode; set { hdrMode = value; OnPropertyChanged(); } }

        [DataMember]
        public ObservableCollection<ApplicationItem> ApplicationItems { get => _applicationItems; set {_applicationItems = value; OnPropertyChanged();} }


        public HDRProfileSettings()
        {
            ApplicationItems = new ObservableCollection<ApplicationItem>();
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
