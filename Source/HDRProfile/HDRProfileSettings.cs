using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HDRProfile
{
    public class HDRProfileSettings : BaseViewModel
    {

        private bool autoStart;
        private bool startMinimizedToTray;
        private bool _closeToTray;
        private HDRMode hdrMode;
        private ObservableCollection<ApplicationItem> _applicationItems;

        public bool AutoStart { get => autoStart; set { autoStart = value; OnPropertyChanged(); } }

        public bool StartMinimizedToTray { get => startMinimizedToTray; set { startMinimizedToTray = value; OnPropertyChanged(); }  }
        public bool CloseToTray { get => _closeToTray; set { _closeToTray = value; OnPropertyChanged(); } }

        public HDRMode HDRMode { get => hdrMode; set { hdrMode = value; OnPropertyChanged(); } }

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
