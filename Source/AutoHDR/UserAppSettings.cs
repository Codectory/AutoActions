using AutoHDR.Displays;
using AutoHDR.Profiles;
using AutoHDR.Profiles.Actions;
using CodectoryCore;
using CodectoryCore.UI.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AutoHDR
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserAppSettings : BaseViewModel
    {
        public static readonly object _settingsLock = new object();


        private bool _globalAutoHDR = true;
        private bool _createLogFile = false;
        private bool _autoStart;
        private bool _startMinimizedToTray;
        private bool _closeToTray;
        private bool _checkForNewVersion = true;
        readonly object _audioDevicesLock = new object();
        private Guid _defaultProfileGuid = Guid.Empty;

        private SortableObservableCollection<ApplicationProfileAssignment> _applicationProfileAssignments;
        private DispatchingObservableCollection<Profile> _applicationProfiles;
        private DispatchingObservableCollection<Display> _displays;
        private DispatchingObservableCollection<ProfileActionShortcut> _actionShortcuts;


        [JsonProperty]
        public Guid DefaultProfileGuid { get => _defaultProfileGuid; set { _defaultProfileGuid = value; OnPropertyChanged(); OnPropertyChanged(nameof(DefaultProfile)); } }

        public Profile DefaultProfile { get => ApplicationProfiles.FirstOrDefault(p => p.GUID.Equals(DefaultProfileGuid)); set { DefaultProfileGuid = value == null ? Guid.Empty : value.GUID; } }


        [JsonProperty]
        public bool GlobalAutoHDR { get => _globalAutoHDR; set { _globalAutoHDR = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool AutoStart { get => _autoStart; set { _autoStart = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool CreateLogFile { get => _createLogFile; set { _createLogFile = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool StartMinimizedToTray { get => _startMinimizedToTray; set { _startMinimizedToTray = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool CloseToTray { get => _closeToTray; set { _closeToTray = value; OnPropertyChanged(); } }

        [JsonProperty]
        public bool CheckForNewVersion { get => _checkForNewVersion; set { _checkForNewVersion = value; OnPropertyChanged(); } }


        [JsonProperty(Order = 2)]
        public SortableObservableCollection<ApplicationProfileAssignment> ApplicationProfileAssignments { get => _applicationProfileAssignments; set { _applicationProfileAssignments = value; OnPropertyChanged(); } }

        [JsonProperty(Order = 1)]
        public DispatchingObservableCollection<Profile> ApplicationProfiles { get => _applicationProfiles; set { _applicationProfiles = value; OnPropertyChanged(); } }


        [JsonProperty]
        public DispatchingObservableCollection<Display> Displays { get => _displays; set { _displays = value; OnPropertyChanged(); } }

        [JsonProperty]
        public DispatchingObservableCollection<ProfileActionShortcut> ActionShortcuts { get => _actionShortcuts; set { _actionShortcuts = value; OnPropertyChanged(); } }

        public UserAppSettings()
        {
            ApplicationProfileAssignments = new SortableObservableCollection<ApplicationProfileAssignment>(new ObservableCollection<ApplicationProfileAssignment>());
            ApplicationProfiles = new DispatchingObservableCollection<Profile>();
            ActionShortcuts = new DispatchingObservableCollection<ProfileActionShortcut>();
            Displays = new DispatchingObservableCollection<Display>();
        }

        public static UserAppSettings ReadSettings(string path)
        {
            UserAppSettings settings = null;

            lock (_settingsLock)
            {

                try
                {
                    string serializedJson = File.ReadAllText(path);
                    serializedJson = UpgradeJson(serializedJson);
                    settings = (UserAppSettings)JsonConvert.DeserializeObject<UserAppSettings>(serializedJson, new JsonSerializerSettings
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
                        Globals.Logs.Add("Loaded deprecated xml settings.", false);
                        return settings;
                    }
                    catch (Exception)
                    {
                    }
                    Globals.Logs.AddException(ex);
                    throw;
                }
            }
            return settings;
        }

        private static string UpgradeJson(string serializedJson)
        {
            serializedJson = serializedJson.Replace("\"$type\": \"AutoHDR.Displays.Display, AutoHDR\"", "\"$type\": \"AutoHDR.Displays.Display, AutoHDR.Displays\"");
            serializedJson = serializedJson.Replace("\"Monitors\": [", "\"Displays\": [");
            return serializedJson;
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
                    Globals.Logs.AddException(ex);
                    throw;
                }
            }
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
