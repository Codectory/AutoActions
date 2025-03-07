using AutoActions.UWP;
using CodectoryCore.UI.Wpf;
using CodectoryCore.Windows;
using CodectoryCore.Windows.FileSystem;
using CodectoryCore.Windows.Icons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoActions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApplicationItem : BaseViewModel, IEquatable<ApplicationItem>
    {
        [JsonProperty]
        public bool PackageError { get; set; } = false;
        private bool _isUWP = false;
        private bool _isUWPWebApp = false;

        private string displayName;
        private string _applicationFilePath;
        private string _applicationName;
        private System.Drawing.Bitmap icon = null;
        //private bool _restartProcess = false;
        private string _uwpFullPackageName = string.Empty;
        private string _uwpFamilyPackageName = string.Empty;
        private string _uwpApplicationID = string.Empty;
        private string _uwpIconPath = string.Empty;
        private string _uwpIdentity = string.Empty;


        [JsonProperty]
        public string DisplayName { get => displayName; set { displayName = value; OnPropertyChanged(); } }
        [JsonProperty]
        public string ApplicationName { get => _applicationName; set { _applicationName = value; OnPropertyChanged(); } }
        [JsonProperty(Order = 1)]
        public string ApplicationFilePath {
            get => _applicationFilePath; 
            set { _applicationFilePath = value;  try {  if (!IsUWP && !IsUWPWepApp)  Icon = IconHelper.GetFileIcon(value); } catch { } OnPropertyChanged(); } }
        // public bool RestartProcess { get => _restartProcess; set { _restartProcess = value; OnPropertyChanged(); } }
        [JsonProperty(Order =0)]
        public bool IsUWP { get => _isUWP; set { _isUWP = value; OnPropertyChanged(); } }
        [JsonProperty(Order = 0)]
        public bool IsUWPWepApp { get => _isUWPWebApp; set { _isUWPWebApp = value; OnPropertyChanged(); } }
        public Bitmap Icon { get => icon; set { icon = value; OnPropertyChanged(); } }
        [JsonProperty(Order = 1)]
        public string UWPFullPackageName { 
            get => _uwpFullPackageName; 
            set { _uwpFullPackageName = value;  OnPropertyChanged(); LoadUWPData(); } 
        }

        [JsonProperty(Order = 0)]
        public string UWPFamilyPackageName
        {
            get => _uwpFamilyPackageName;
            set { _uwpFamilyPackageName = value; OnPropertyChanged(); }
        }

        [JsonProperty(Order = 2)]
        public string UWPApplicationID { 
            get => _uwpApplicationID; 
            set { _uwpApplicationID = value; OnPropertyChanged(); if (string.IsNullOrEmpty(UWPFullPackageName)) LoadUWPData(); } }
        public string UWPIconPath { 
            get => _uwpIconPath; 
            set { _uwpIconPath = value; try { if (IsUWP ||IsUWPWepApp) Icon = new Bitmap(Bitmap.FromFile(value)); } catch { }OnPropertyChanged(); } }

        [JsonProperty(Order = 0)]
        public string UWPIdentity { get => _uwpIdentity; set { _uwpIdentity = value; OnPropertyChanged(); } }

        private ApplicationItem()
        {

        }
        public ApplicationItem(string displayName, string applicationFilePath)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            ApplicationFilePath = applicationFilePath ?? throw new ArgumentNullException(nameof(applicationFilePath));
            ApplicationName = new FileInfo(ApplicationFilePath).Name.Replace(".exe", "");
        }

        public ApplicationItem(UWPApp uwpApp) : this(uwpApp.Name, uwpApp.ExecutablePath)
        {
            IsUWP = true;
            IsUWPWepApp = true;
            UWPFamilyPackageName = uwpApp.FamilyPackageName;
            _uwpFullPackageName = uwpApp.FullPackageName;
            UWPIconPath = uwpApp.IconPath;
            UWPApplicationID = uwpApp.ApplicationID;
            UWPIdentity = uwpApp.Identity;
        }

        private void LoadUWPData()
        {
            string packageNotFound = "[PackageNotFound]_";

            if (!IsUWP && !IsUWPWepApp)
                return;
            UWPApp uwpApp;
            uwpApp = UWPAppsManager.GetUWPApp(UWPFamilyPackageName, UWPIdentity);
            if (uwpApp == null)
            {
                if (PackageError)
                    return;
                DisplayName = $"{packageNotFound}{DisplayName}";
                UWPIconPath = "";
                UWPIdentity = "";
                PackageError = true;
                return;
            }
            PackageError = false;
            if (DisplayName.StartsWith(packageNotFound))
                DisplayName = DisplayName.Substring(packageNotFound.Length, DisplayName.Length - packageNotFound.Length);
            UWPFamilyPackageName = uwpApp.FamilyPackageName;
            _uwpFullPackageName = uwpApp.FullPackageName;
            UWPIconPath = uwpApp.IconPath;
            UWPIdentity = uwpApp.Identity;
        }

        Dictionary<ApplicationItem, ApplicationState> _lastAppStates = new Dictionary<ApplicationItem, ApplicationState>();

        public void Restart()
        {
            try
            {
                Globals.Logs.Add($"Restarting application {ApplicationName}", false);
                foreach (Process process in Process.GetProcessesByName(ApplicationName).ToList())
                    if (process.StartTime < Process.GetCurrentProcess().StartTime)
                    {
                        Globals.Logs.Add($"Won't restart application {ApplicationName} as it was running before { ProjectResources.ProjectLocales.AutoActions}.", false);

                        return;
                    }
                Process.GetProcessesByName(ApplicationName).ToList().ForEach(p => p.Kill());
                System.Threading.Thread.Sleep(1500);
                StartApplication();
            }
            catch (Exception ex)
            {
                Globals.Logs.AddException($"Failed to restart process {DisplayName} ({ApplicationFilePath}).", ex);
                throw;
            }
        }

        public void StartApplication()
        {
            Globals.Logs.Add($"Start application {ApplicationName}", false);
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
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(ApplicationFilePath);
                    process.Start();
                }
                System.Threading.Thread.Sleep(2500);
                var processes = Process.GetProcessesByName(ApplicationName).ToList();
                if (processes.Count > 0)
                {
                    Process foundProcess = new Process();
                    Globals.Logs.Add($"Bring application to front: {ApplicationName}", false);
                    foundProcess = processes[0];
                    if (!foundProcess.HasExited && foundProcess.Responding)
                        Window.BringMainWindowToFront(foundProcess.ProcessName);
                }
                else
                    Globals.Logs.Add($"No started application found: {ApplicationName}", false);

            }
            catch (Exception ex)
            {
                Globals.Logs.AddException(ex);
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
