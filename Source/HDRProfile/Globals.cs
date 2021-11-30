using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR
{
    public class Globals : BaseViewModel
    {
        public static int GlobalRefreshInterval = 500;

        private string SettingsPathCompatible => $"{System.AppDomain.CurrentDomain.BaseDirectory}HDRProfile_Settings.xml";

        private string SettingsPathCompatible2 => $"{System.AppDomain.CurrentDomain.BaseDirectory}UserSettings.xml";

        private string SettingsPath => $"{System.AppDomain.CurrentDomain.BaseDirectory}UserSettings.json";


        public static Globals Instance = new Globals();

        private UserAppSettings _settings;
        public UserAppSettings Settings { get => _settings; set { _settings = value; OnPropertyChanged(); } }
        private bool _settingsLoadedOnce = false;

        public void SaveSettings(bool force = false)
        {
            if (!force && !_settingsLoadedOnce)
                return;
            Tools.Logs.Add("Saving settings..", false);
            try
            {
                Settings.SaveSettings(SettingsPath);
                Tools.Logs.Add("Settings saved", false);

            }
            catch (Exception ex)
            {
                Tools.Logs.AddException(ex);
            }
        }

        public void LoadSettings()
        {
            try
            {
                Tools.Logs.Add("Loading settings...", false);
                if (File.Exists(SettingsPath))
                {
                    Settings = UserAppSettings.ReadSettings(SettingsPath);
                    _settingsLoadedOnce = true;
                }
                else if (File.Exists(SettingsPathCompatible2))
                {
                    Settings = UserAppSettings.ReadSettings(SettingsPathCompatible2);
                    _settingsLoadedOnce = true;
                }
                else if (File.Exists(SettingsPathCompatible))
                {
                    Settings = LoadObsoleteHDRSettings();
                    _settingsLoadedOnce = true;
                    File.Delete(SettingsPathCompatible);
                }
                else
                {
                    Tools.Logs.Add("No settings found. Creating settings file...", false);
                    Settings = new UserAppSettings();
                    _settingsLoadedOnce = true;

                }

                SaveSettings();

            }
            catch (Exception ex)
            {
                string backupFile = $"{System.AppDomain.CurrentDomain.BaseDirectory}Backup_Settings_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xml.bak";
                if (File.Exists(SettingsPath))
                {
                    File.Move(SettingsPath, backupFile);
                    Tools.Logs.Add($"Created backup of invalid settings file: {backupFile}", false);
                    File.Delete(SettingsPath);
                }
                Tools.Logs.Add("Failed to load settings", false);
                Tools.Logs.AddException(ex);
                Settings = new UserAppSettings();
                SaveSettings(true);
                Tools.Logs.Add("Created new settings file", false);
            }
            Logs.LoggingEnabled = Settings.Logging;
            Tools.Logs.Add("Settings loaded", false);
        }

        [Obsolete]
        private UserAppSettings LoadObsoleteHDRSettings()
        {
            return UserAppSettings.Convert(HDRProfileSettings.ReadSettings(SettingsPathCompatible));
        }
    }
}
