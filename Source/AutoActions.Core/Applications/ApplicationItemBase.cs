using AutoActions.Core;
using CodectoryCore.UI.Wpf;
using CodectoryCore.Windows.Icons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AutoActions
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ApplicationItemBase : BaseViewModel, IEquatable<ApplicationItemBase>
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
            set { _applicationFilePath = value;  try { Icon = IconHelper.GetFileIcon(value); } catch { } OnPropertyChanged(); } }
        public Bitmap Icon { get => icon; set { icon = value; OnPropertyChanged(); } }



        Type _provider;

        [JsonProperty(Order = 0)]

        public Type Provider { get => _provider; set { _provider = value; OnPropertyChanged(); } }


        protected ApplicationItemBase()
        {

        }
        public ApplicationItemBase(string displayName, string applicationFilePath, Type _provider)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            ApplicationFilePath = applicationFilePath ?? throw new ArgumentNullException(nameof(applicationFilePath));
            ApplicationName = new FileInfo(ApplicationFilePath).Name.Replace(".exe", "");
            Provider = _provider;
        }

        Dictionary<ApplicationItemBase, ApplicationState> _lastAppStates = new Dictionary<ApplicationItemBase, ApplicationState>();

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

        public abstract void StartApplication();

        #region Overrides 

        public override bool Equals(object obj)
        {
            return Equals(obj as ApplicationItemBase);
        }

        public bool Equals(ApplicationItemBase other)
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

        public static bool operator ==(ApplicationItemBase left, ApplicationItemBase right)
        {
            return EqualityComparer<ApplicationItemBase>.Default.Equals(left, right);
        }

        public static bool operator !=(ApplicationItemBase left, ApplicationItemBase right)
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
