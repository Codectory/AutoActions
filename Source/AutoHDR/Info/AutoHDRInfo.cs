using CodectoryCore.UI.Wpf;
using AutoHDR.Info.Github;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CodectoryCore;
using AutoHDR.ProjectResources;

namespace AutoHDR.Info
{
    public class AutoHDRInfo : DialogViewModelBase
    {
        private GitHubData _gitHubData = null;

        private string _applicationTitle;
        public string ApplicationTitle
        {
            get { return _applicationTitle; }
            set { _applicationTitle = value; OnPropertyChanged(); OnPropertyChanged(nameof(NewUpdateAvailabe)); }
        }

        private Version _version;

        public Version Version
        {
            get { return _version; }
            set { _version = value; OnPropertyChanged(); }
        }

        private Version _newestVersion;

        public Version NewestVersion
        {
            get { return _newestVersion; }
            set { _newestVersion = value; OnPropertyChanged(); OnPropertyChanged(nameof(NewUpdateAvailabe)); }
        }

        private string _changelog;

        public string ChangeLog
        {
            get { return _changelog; }
            set { _changelog = value; OnPropertyChanged(); }
        }

        private string _downloadLink;

        public string DownloadLink
        {
            get { return _downloadLink; }
            set { _downloadLink = value; OnPropertyChanged(); }
        }

        private DateTime _lastReleaseDate;

        public DateTime LastReleaseDate
        {
            get { return _lastReleaseDate; }
            set { _lastReleaseDate = value; OnPropertyChanged(); }
        }


        public bool NewUpdateAvailabe
        {
            get 
            {
                return Version.CompareTo(NewestVersion) < 0;
            }
        }



        private Image _logo;

        public Image Logo
        {
            get { return _logo; }
            set { _logo = value; }
        }

        public RelayCommand LoadedCommand { get; private set; }
        public RelayCommand BuyBeerCommand { get; private set; }
        public RelayCommand OpenGitHubCommand { get; private set; }

        public RelayCommand DownloadCommand { get; private set; }



        public AutoHDRInfo()
        {
            CreateRelayCommands();
            Version = VersionExtension.ApplicationVersion(System.Reflection.Assembly.GetExecutingAssembly());
            Title = ProjectLocales.Info;
        }

        private void CreateRelayCommands()
        {
            LoadedCommand = new RelayCommand(LoadingGitHubData);
            BuyBeerCommand = new RelayCommand(BuyBeer);
            OpenGitHubCommand = new RelayCommand(OpenGitHub);
            DownloadCommand = new RelayCommand(OpenDownloadLink);
        }

        public AutoHDRInfo(GitHubData gitHubData) : this()
        {
            _gitHubData = gitHubData;

        }

        private void LoadingGitHubData()
        {
            if (_gitHubData == null)
                _gitHubData = GitHubIntegration.GetGitHubData();
            NewestVersion = _gitHubData.CurrentVersion;
            ChangeLog = _gitHubData.ChangeLog;
            LastReleaseDate = _gitHubData.LastReleaseDate;
            DownloadLink = _gitHubData.DownloadLink;
        }

        private void BuyBeer()
        {
            Process.Start(new ProcessStartInfo((string)Application.Current.Resources["DonateLink"]));
        }

        private void OpenGitHub()
        {
            Process.Start(new ProcessStartInfo((string)Application.Current.Resources["GitHubRepoLink"]));
        }

        private void OpenDownloadLink()
        {
            Process.Start(new ProcessStartInfo(DownloadLink));
        }

    }
}
