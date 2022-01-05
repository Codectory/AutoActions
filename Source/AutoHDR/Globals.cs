using AutoHDR.Profiles;
using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR
{
    public class Globals : BaseViewModel
    {

        public static Logs Logs = new Logs($"{System.AppDomain.CurrentDomain.BaseDirectory}AutoHDR.log", "AutoHDR", Assembly.GetExecutingAssembly().GetName().Version.ToString(), false);

        public static int GlobalRefreshInterval = 500;

        private string SettingsPathCompatible => $"{System.AppDomain.CurrentDomain.BaseDirectory}UserSettings.xml";

        private string SettingsPath => $"{System.AppDomain.CurrentDomain.BaseDirectory}UserSettings.json";


        public static Globals Instance = new Globals();

        private UserAppSettings _settings;
        public UserAppSettings Settings { get => _settings; set { _settings = value; OnPropertyChanged(); } }
        public bool SettingsLoadedOnce { get; private set; } = false;

        public event EventHandler SettingsLoaded;

        public void SaveSettings(bool force = false)
        {
            if (!force && !SettingsLoadedOnce)
                return;
            Globals.Logs.Add("Saving settings..", false);
            try
            {
                Settings.SaveSettings(SettingsPath);
                Globals.Logs.Add("Settings saved", false);
            }
            catch (Exception ex)
            {
                Globals.Logs.AddException(ex);
            }
        }

        public void LoadSettings()
        {
            try
            {
                Globals.Logs.Add("Loading settings...", false);
                if (File.Exists(SettingsPath))
                {
                    Settings = UserAppSettings.ReadSettings(SettingsPath);
                    SettingsLoadedOnce = true;
                }
                else if (File.Exists(SettingsPathCompatible))
                {
                    Settings = UserAppSettings.ReadSettings(SettingsPathCompatible);
                    SettingsLoadedOnce = true;
                }
                else
                {
                    Globals.Logs.Add("No settings found. Creating settings file...", false);
                    Settings = new UserAppSettings();
                    Settings.ApplicationProfiles.Add(Profile.DefaultProfile());
                   SettingsLoadedOnce = true;
                }
                SaveSettings();
                SettingsLoaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                string backupFile = $"{System.AppDomain.CurrentDomain.BaseDirectory}Backup_Settings_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.json";
                if (File.Exists(SettingsPath))
                {
                    File.Move(SettingsPath, backupFile);
                    Globals.Logs.Add($"Created backup of invalid settings file: {backupFile}", false);
                    File.Delete(SettingsPath);
                }
                Globals.Logs.Add("Failed to load settings", false);
                Globals.Logs.AddException(ex);
                Settings = new UserAppSettings();
                SaveSettings(true);
                Globals.Logs.Add("Created new settings file", false);
            }
            Globals.Logs.LogFileEnabled = Settings.CreateLogFile;
            Globals.Logs.Add("Settings loaded", false);
        }
    }
}
