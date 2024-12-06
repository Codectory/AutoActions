using AutoActions.Core;
using AutoActions.Info;
using AutoActions.Info.Github;
using AutoActions.Profiles;
using CodectoryCore;
using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoActions
{
    public class ProjectData : BaseViewModel
    {


        public static int GlobalRefreshInterval = 500;

        private string SettingsPathCompatible => $"{System.AppDomain.CurrentDomain.BaseDirectory}UserSettings.xml";

        private string SettingsPath => $"{System.AppDomain.CurrentDomain.BaseDirectory}UserSettings.json";


        public static ProjectData Instance = new ProjectData();

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
                FixAssignments();
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

        private void FixAssignments()
        {
            int count = Settings.ApplicationProfileAssignments.Count;
            for (int i = 0; i < count; i++)
            {
                int positionCount = Settings.ApplicationProfileAssignments.Count(a => a.Position == i);
                if (positionCount == 0)
                {
                    int u = i;
                    while (Settings.ApplicationProfileAssignments.Count(a => a.Position == i) == 0)
                    {
                        var assignemnt = Settings.ApplicationProfileAssignments.FirstOrDefault(a => a.Position == u);
                        if (assignemnt != null)
                            assignemnt.Position = i;
                        u++;
                    }
                }
                if (positionCount > 1)
                    Settings.ApplicationProfileAssignments.First(a => a.Position == i).Position = i + 1;
            }
            while (Settings.ApplicationProfileAssignments.Any(a => a.Position >= count))
            {
                foreach (var assignment in Settings.ApplicationProfileAssignments)
                    if (assignment.Position >= count)
                        do
                        {
                            assignment.Position = assignment.Position - 1;
                        } while (Settings.ApplicationProfileAssignments.Count(a => a.Position == assignment.Position) > 1);
            }
        }



        public CheckUpdateResult CheckUpdate()
        {

            Globals.Logs.Add($"Checking for new version...", false);
            GitHubData data = null;
            try
            {
                data = GitHubIntegration.GetGitHubData();
            }
            catch (Exception ex)
            {
                Globals.Logs.AddException(ex);
                return new CheckUpdateResult(false, data);
            }
            Version localVersion = VersionExtension.ApplicationVersion(System.Reflection.Assembly.GetExecutingAssembly());
            int versionComparison = localVersion.CompareTo(data.CurrentVersion);
            if (versionComparison < 0)
            {
                Globals.Logs.Add($"Newer version availabe.", false);
                if (!Settings.AutoUpdate)
                {
                    Application.Current.Dispatcher.Invoke(
                      (Action)(() =>
                      {
                          ShowInfo(data);
                      }));
                }
            }
            else
                Globals.Logs.Add($"Local version is up to date.", false);
            return new CheckUpdateResult(versionComparison < 0, data);

        }

        public void AutoUpdate(GitHubData data)
        {
            Globals.Logs.Add($"Updating AutoActions to {data.CurrentVersion}...", false);

            DirectoryInfo applicationPath = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory);
            string updatePath = $"{applicationPath.FullName}\\Update";
            if (Directory.Exists(updatePath))
                Directory.Delete(updatePath, true);
            DirectoryCopy(applicationPath.FullName, updatePath, true);
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(Path.Combine(updatePath, "AutoActions.Updater.exe"));
            process.StartInfo.Arguments = $"true \"{ (Environment.Is64BitOperatingSystem ? data.DirectDownload64 : data.DirectDownload86)}\" \"{applicationPath.FullName.Substring(0, applicationPath.FullName.Length-1)}\" \"{Process.GetCurrentProcess().ProcessName}\"";
            process.Start();

            App.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo targetDir = new DirectoryInfo(destDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    if (subdir.FullName.Equals(targetDir.FullName))
                        continue;
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        public void ShowInfo()
        {
            ShowInfo(null);
        }
        private void ShowInfo(GitHubData data)
        {
            AutoActionsInfo info;
            if (data == null)
                info = new AutoActionsInfo();
            else
                info = new AutoActionsInfo(data);
            if (DialogService != null)
                DialogService.ShowDialogModal(info, new System.Drawing.Size(600, 1000));
        }
    }
}
