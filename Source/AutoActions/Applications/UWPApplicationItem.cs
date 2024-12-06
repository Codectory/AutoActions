using AutoActions.Core;
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
    public class UWPApplicationItem : ApplicationItemBase
    {
        [JsonProperty]
        private bool _isUWP = false;
        private bool _isUWPWebApp = false;

        private string _uwpFullPackageName = string.Empty;
        private string _uwpFamilyPackageName = string.Empty;
        private string _uwpApplicationID = string.Empty;
        private string _uwpIconPath = string.Empty;
        private string _uwpIdentity = string.Empty;


        [JsonProperty(Order = 0)]
        public bool IsUWPWepApp { get => _isUWPWebApp; set { _isUWPWebApp = value; OnPropertyChanged(); } }


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
            set { _uwpIconPath = value; try { Icon = new Bitmap(Bitmap.FromFile(value)); } catch { }OnPropertyChanged(); } }

        [JsonProperty(Order = 0)]
        public string UWPIdentity { get => _uwpIdentity; set { _uwpIdentity = value; OnPropertyChanged(); } }

        private UWPApplicationItem()
        {
            Provider = typeof(UWPAppsManager);
        }

        public UWPApplicationItem(UWPApp uwpApp) : base(uwpApp.Name, uwpApp.ExecutablePath, typeof(UWPAppsManager))
        {
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

            if (!IsUWPWepApp)
                return;
            UWPApp uwpApp;
            uwpApp = UWPAppsManager.Instance.GetUWPApp(UWPFamilyPackageName, UWPIdentity);
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



        public override void StartApplication()
        {
            Globals.Logs.Add($"Start application {ApplicationName}", false);
            try
            {

                UWP.UWPAppsManager.Instance.StartUWPApp(UWPFamilyPackageName, UWPApplicationID);

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
    }
}
