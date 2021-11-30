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
using System.Xml;
using Newtonsoft.Json;

namespace AutoHDR
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserAppSettings : BaseViewModel
    {
        public static readonly object _settingsLock = new object();


        private bool _globalAutoHDR = true;
        private bool _logging = false;
        private bool _autoStart;
        private bool _startMinimizedToTray;
        private bool _closeToTray;
        private bool _checkForNewVersion = true;
        readonly object _audioDevicesLock = new object();
        private SortableObservableCollection<ApplicationProfileAssignment> _applicationProfileAssignments;
        private ObservableCollection<Profile> _applicationProfiles;
        private ObservableCollection<Display> _monitors;



        [JsonProperty]
        public bool GlobalAutoHDR { get => _globalAutoHDR; set { _globalAutoHDR = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool AutoStart { get => _autoStart; set { _autoStart = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool Logging { get => _logging; set { _logging = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool StartMinimizedToTray { get => _startMinimizedToTray; set { _startMinimizedToTray = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool CloseToTray { get => _closeToTray; set { _closeToTray = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool CheckForNewVersion { get => _checkForNewVersion; set { _checkForNewVersion = value; OnPropertyChanged(); } }


        [JsonProperty(Order = 1)]
        public SortableObservableCollection<ApplicationProfileAssignment> ApplicationProfileAssignments { get => _applicationProfileAssignments; set { _applicationProfileAssignments = value; OnPropertyChanged(); } }

        [JsonProperty(Order = 0)]
        public ObservableCollection<Profile> ApplicationProfiles { get => _applicationProfiles; set { _applicationProfiles = value; OnPropertyChanged(); } }


        [JsonProperty]
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

            lock (_settingsLock)
            {

                try
                {
                    string serializedJson = File.ReadAllText(path);
                    settings =(UserAppSettings) JsonConvert.DeserializeObject<UserAppSettings>(serializedJson, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                    });          
                }
                catch (Exception ex)
                {
                    try
                    {
                        settings = TryReadXML(path);
                        Tools.Logs.Add("Loaded deprecated xml settings.", false);
                        return settings;
                    }
                    catch (Exception)
                    {
                    }
                    Tools.Logs.AddException(ex);
                    throw;
                }
            }
            return settings;
        }

        private static UserAppSettings TryReadXML(string path)
        {
            UserAppSettings settings = null;
            XmlSerializer serializer = new XmlSerializer(typeof(UserAppSettings));
            using (TextReader reader = new StreamReader(path))
            {
                settings = (UserAppSettings)serializer.Deserialize(reader);
            }
            return settings;
        }

        public static void SaveSettings(UserAppSettings settings, string path)
        {
            lock (_settingsLock)
            {
                try
                {
                    string serializedJson = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                    });
                    File.WriteAllText(path, serializedJson);
                }
                catch (Exception ex)
                {
                    Tools.Logs.AddException(ex);
                    throw;
                }
            }
        }

        [Obsolete]
        public static UserAppSettings Convert(HDRProfileSettings settings)
        {
            UserAppSettings convertedSettings = new UserAppSettings();
            convertedSettings.AutoStart = settings.AutoStart;
            convertedSettings.CheckForNewVersion = settings.CheckForNewVersion;
            convertedSettings.CloseToTray = settings.CloseToTray;
            convertedSettings.GlobalAutoHDR = settings.GlobalAutoHDR;
            convertedSettings.Logging = settings.Logging;
            convertedSettings.Monitors = settings.Monitors;
            convertedSettings.StartMinimizedToTray = settings.StartMinimizedToTray;
            return convertedSettings;
        }
    }

    public static class UserAppSettingsExtension
    {

        public static void SaveSettings(this UserAppSettings settings, string path)
        {
            UserAppSettings.SaveSettings(settings, path);
        }

    }

}
