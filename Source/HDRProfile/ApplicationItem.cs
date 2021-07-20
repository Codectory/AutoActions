using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoHDR
{
    public class ApplicationItem : BaseViewModel, IEquatable<ApplicationItem>
    {
        private bool _isUWP = false;
        private string displayName;
        private string _applicationFilePath;
        private string _applicationName;
        private System.Drawing.Bitmap icon = null;
        //private bool _restartProcess = false;
        private string _uwpFamilyPackageName;
        private string _uwpApplicationID;
        private string _uwpIconPath;


        public string DisplayName { get => displayName; set { displayName = value; OnPropertyChanged(); } }
        public string ApplicationName { get => _applicationName; set { _applicationName = value; OnPropertyChanged(); } }
        public string ApplicationFilePath { get => _applicationFilePath; set { _applicationFilePath = value;  try { Icon = Tools.GetFileIcon(value); } catch { } OnPropertyChanged(); } }
       // public bool RestartProcess { get => _restartProcess; set { _restartProcess = value; OnPropertyChanged(); } }
        public bool IsUWP { get => _isUWP; set { _isUWP = value; OnPropertyChanged(); } }

        [XmlIgnore]
        public Bitmap Icon { get => icon; set { icon = value; OnPropertyChanged(); } }

        public string UWPFamilyPackageName { get => _uwpFamilyPackageName; set { _uwpFamilyPackageName = value; OnPropertyChanged(); } }

        public string UWPApplicationID { get => _uwpApplicationID; set { _uwpApplicationID = value; OnPropertyChanged(); } }
        public string UWPIconPath { get => _uwpIconPath; set { _uwpIconPath = value; try { Icon = new Bitmap(Bitmap.FromFile(value)); } catch { }OnPropertyChanged(); } }


        private ApplicationItem()
        {

        }
        public ApplicationItem(string displayName, string applicationFilePath)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            ApplicationFilePath = applicationFilePath ?? throw new ArgumentNullException(nameof(applicationFilePath));
            ApplicationName = new FileInfo(ApplicationFilePath).Name.Replace(".exe", "");
        }

        public ApplicationItem(string displayName, string applicationFilePath, string uwpFamilyPackageName, string uwpApplicationID, string iconPath = "") : this(displayName, applicationFilePath)
        {
            IsUWP = true;
            UWPFamilyPackageName = uwpFamilyPackageName;
            UWPApplicationID = uwpApplicationID;
            UWPIconPath = iconPath;
        }


        Dictionary<ApplicationItem, ApplicationState> _lastAppStates = new Dictionary<ApplicationItem, ApplicationState>();


        //private void UpdateRestartAppStates(IDictionary<ApplicationItem, ApplicationState> applicationStates, bool restartApps)
        //{
        //    Dictionary<ApplicationItem, ApplicationState> newLastAppStates = new Dictionary<ApplicationItem, ApplicationState>();
        //    Tools.Logs.Add($"Updating application states...", false);
        //    foreach (var applicationState in applicationStates)
        //    {
        //        newLastAppStates.Add(applicationState.Key, applicationState.Value);

        //        if (applicationState.Key.RestartProcess && restartApps)
        //        {
        //            if (!_lastAppStates.ContainsKey(applicationState.Key) && applicationState.Value != ApplicationState.None)
        //                RestartProcess(applicationState.Key);
        //            else if (_lastAppStates.ContainsKey(applicationState.Key) && applicationState.Value != ApplicationState.None && _lastAppStates[applicationState.Key] == ApplicationState.None)
        //                RestartProcess(applicationState.Key);
        //        }
        //    }
        //    _lastAppStates.Clear();
        //    _lastAppStates = newLastAppStates;
        //}

        public void Restart()
        {
            try
            {
                Tools.Logs.Add($"Restarting application {ApplicationName}", false);
                foreach (Process process in Process.GetProcessesByName(ApplicationName).ToList())
                    if (process.StartTime < Process.GetCurrentProcess().StartTime)
                    {
                        Tools.Logs.Add($"Won't restart application {ApplicationName} as it was running before { ProjectResources.Locale_Texts.AutoHDR}.", false);

                        return;
                    }
                Process.GetProcessesByName(ApplicationName).ToList().ForEach(p => p.Kill());
                System.Threading.Thread.Sleep(1500);
                StartApplication();
            }
            catch (Exception ex)
            {
                Tools.Logs.AddException($"Failed to restart process {DisplayName} ({ApplicationFilePath}).", ex);
                throw;
            }
        }

        public void StartApplication()
        {
            Tools.Logs.Add($"Start application {ApplicationName}", false);
            try
            {
                if (IsUWP)
                {
                    UWP.UWPAppsManager.StartUWPApp(UWPFamilyPackageName, UWPApplicationID);
                }
                else
                {
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo(ApplicationFilePath);
                    process.Start();
                }
                System.Threading.Thread.Sleep(2500);
                var processes = Process.GetProcessesByName(ApplicationName).ToList();
                if (processes.Count > 0)
                {
                    Process foundProcess = new Process();
                    Tools.Logs.Add($"Bring application to front: {ApplicationName}", false);
                    foundProcess = processes[0];
                    if (!foundProcess.HasExited && foundProcess.Responding)
                        Tools.BringMainWindowToFront(foundProcess.ProcessName);
                }
                else
                    Tools.Logs.Add($"No started application found: {ApplicationName}", false);

            }
            catch (Exception ex)
            {
                Tools.Logs.AddException(ex);
            }
        }



        #region Overrides 

        public override bool Equals(object obj)
        {
            return Equals(obj as ApplicationItem);
        }

        public bool Equals(ApplicationItem other)
        {
            return other != null &&
                   _applicationFilePath == other._applicationFilePath;
        }

        public override int GetHashCode()
        {
            int hashCode = 734317580;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_applicationFilePath);
            return hashCode;
        }

        public static bool operator ==(ApplicationItem left, ApplicationItem right)
        {
            return EqualityComparer<ApplicationItem>.Default.Equals(left, right);
        }

        public static bool operator !=(ApplicationItem left, ApplicationItem right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{DisplayName} [{ApplicationName} |{ApplicationFilePath}]";
        }

        #endregion Overrides
    }
}
