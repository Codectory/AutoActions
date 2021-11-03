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
    public  class Globals : BaseViewModel
    {
        private string SettingsPathCompatible => $"{System.AppDomain.CurrentDomain.BaseDirectory}HDRProfile_Settings.xml";
        private string SettingsPath => $"{System.AppDomain.CurrentDomain.BaseDirectory}UserSettings.xml";


        public static Globals Instance = new Globals();

        private UserAppSettings _settings;
        public UserAppSettings Settings { get => _settings; set { _settings = value; OnPropertyChanged(); } }

        public void SaveSettings()
        {
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
                if (File.Exists(SettingsPath))
                {
                    Tools.Logs.Add("Loading settings...", false);
                    Settings = UserAppSettings.ReadSettings(SettingsPath);
                }
                else if (File.Exists(SettingsPathCompatible))
                {
                    Tools.Logs.Add("Loading settings...", false);
                    Settings = UserAppSettings.Convert(HDRProfileSettings.ReadSettings(SettingsPathCompatible));
                    File.Delete(SettingsPathCompatible);
                }
                else
                {
                    Tools.Logs.Add("Creating settings file", false);
                    Settings = new UserAppSettings();
                    SaveSettings();
                }
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
                SaveSettings();
                Tools.Logs.Add("Created new settings file", false);
            }
            Logs.LoggingEnabled = Settings.Logging;
            Tools.Logs.Add("Settings loaded", false);
        }
    }
}
