using AutoHDR.Displays;
using AutoHDR.Info;
using AutoHDR.Info.Github;
using AutoHDR.Profiles;
using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoHDR
{
    public class AutoHDRDaemon : BaseViewModel
    {
        readonly object _accessLock = new object();
        private bool _showView = false;
        private ApplicationItem _currentApplication = null;
        private Profile _currentProfile = null;
        
        private bool _hdrIsActive;


        private bool started = false;
        public bool Started { get => started; private set { started = value; OnPropertyChanged(); } }
        ProcessWatcher ApplicationWatcher;
        TrayMenuHelper TrayMenuHelper;


        #region RelayCommands

        public RelayCommand ActivateHDRCommand { get; private set; }
        public RelayCommand DeactivateHDRCommand { get; private set; }
        public RelayCommand AddAssignmentCommand { get; private set; }
        public RelayCommand<ApplicationProfileAssignment> RemoveAssignmentCommand { get; private set; }

        public RelayCommand<ApplicationProfileAssignment> MoveAssignmentUpCommand { get; private set; }
        public RelayCommand<ApplicationProfileAssignment> MoveAssignmentDownCommand { get; private set; }

        public RelayCommand AddProfileCommand { get; private set; }
        public RelayCommand<Profile> RemoveProfileCommand { get; private set; }
        public RelayCommand ShowInfoCommand { get; private set; }

        public RelayCommand LoadingCommand { get; private set; }
        public RelayCommand ClosingCommand { get; private set; }
        public RelayCommand ShutdownCommand { get; private set; }
        public RelayCommand BuyBeerCommand { get; private set; }

        public RelayCommand<ApplicationItem> StartApplicationCommand { get; private set; }

        #endregion RelayCommands

        public UserAppSettings Settings { get => Globals.Instance.Settings; set { Globals.Instance.Settings = value; OnPropertyChanged(); } }
        public Profile CurrentProfile { get => _currentProfile; set { _currentProfile = value; OnPropertyChanged(); } }


        public bool Initialized { get; private set; } = false;
        public bool ShowView { get => _showView; set { _showView = value; OnPropertyChanged(); } }

        public ApplicationItem CurrentApplication { get => _currentApplication; set { _currentApplication = value; OnPropertyChanged(); } }

        public bool HDRIsActive { get => _hdrIsActive; set { _hdrIsActive = value; OnPropertyChanged(); } }
        public Version Version
        {
            get
            {
                return Tools.ApplicationVersion;
            }
        }


        public AutoHDRDaemon()
        {
            //ChangeLanguage( new System.Globalization.CultureInfo("en-US"));
            Initialize();
        }

        private void ChangeLanguage(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture.Name);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(culture.Name);
        }

        #region Initialization

        private void Initialize()
        {

            lock (_accessLock)
            {
                if (Initialized)
                    return;
                ApplicationWatcher = new ProcessWatcher();
                ApplicationWatcher.NewLog += ApplicationWatcher_NewLog;
                ApplicationWatcher.ApplicationChanged += ApplicationWatcher_ApplicationChanged;
                LoadSettings();
                Tools.Logs.Add("Initializing...", false);

                if (Settings.CheckForNewVersion)
                    CheckForNewVersion();
                InitializeDisplayManager();
                Globals.Instance.SaveSettings(); 
                InitializeTrayMenuHelper();
                CreateRelayCommands();
                ShowView = !Settings.StartMinimizedToTray;
                Initialized = true;
                Tools.Logs.Add("Initialized", false);
                Start();
                Tools.Logs.Add($"Starting process watcher...", false);
                ApplicationWatcher.Start();
                Tools.Logs.Add($"Process watcher started", false);

            }
        }

        private void ApplicationWatcher_ApplicationChanged(object sender, ApplicationChangedEventArgs e)
        {
            Tools.Logs.Add($"Application {e.Application} changed: {e.ChangedType}", false);
            CurrentApplication = e.Application;
            UpdateCurrentProfile(e.Application, e.ChangedType);
            if (e.ChangedType == ApplicationChangedType.Closed)
                CurrentApplication = null;
        }

        private void ApplicationWatcher_NewLog(object sender, string e)
        {
            Tools.Logs.Add(e, false);
        }

        private void UpdateCurrentProfile(ApplicationItem application, ApplicationChangedType changedType)
        {
            lock (_accessLock)
            {
                ApplicationProfileAssignment assignment = Settings.ApplicationProfileAssignments.First(a => a.Application.ApplicationFilePath.Equals(application.ApplicationFilePath));

                if (assignment == null)
                {
                    Tools.Logs.Add($"No assignmet for {application.ApplicationFilePath}.", false);
                    CurrentProfile = null;
                    return;
                }
                Profile profile = assignment.Profile;


                if (profile == null)
                    return;
                Tools.Logs.Add($"Profile changed to {profile.Name}", false);

                switch (changedType)
                {
                    case ApplicationChangedType.Started:
                        foreach (var action in profile.ApplicationStarted)
                            action.RunAction();
                        break;
                    case ApplicationChangedType.Closed:
                        foreach (var action in profile.ApplicationClosed)
                            action.RunAction();
                        break;
                    case ApplicationChangedType.GotFocus:
                        foreach (var action in profile.ApplicationGotFocus)
                            action.RunAction();
                        break;
                    case ApplicationChangedType.LostFocus:
                        foreach (var action in profile.ApplicationLostFocus)
                            action.RunAction();
                        break;
                }
                if (profile.RestartApplication && changedType == ApplicationChangedType.Started)
                    assignment.Application.Restart();
                CurrentProfile = profile;
                if (changedType == ApplicationChangedType.Closed)
                    CurrentProfile = null;
            }
        }

        private void CheckForNewVersion()
        {
            Task.Run(() =>
            {
                Tools.Logs.Add($"Checking for new version...", false);

                GitHubData data = GitHubIntegration.GetGitHubData();
                Version localVersion = Tools.ApplicationVersion;
                int versionComparison = localVersion.CompareTo(data.CurrentVersion);
                if (versionComparison < 0)
                {
                    Tools.Logs.Add($"Newer version availabe.", false);

                    Application.Current.Dispatcher.Invoke(
                      (Action)(() =>
                      {
                          ShowInfo(data);
                      }));

                }
                else
                    Tools.Logs.Add($"Local version is up to date.", false);

            });
        }


        private void InitializeTrayMenuHelper()
        {
            TrayMenuHelper = new TrayMenuHelper();
            TrayMenuHelper.Initialize(DisplayManager.Instance);
            TrayMenuHelper.OpenViewRequested += TrayMenuHelper_OpenViewRequested;
            TrayMenuHelper.CloseApplicationRequested += TrayMenuHelper_CloseApplicationRequested;
            //TrayMenuHelper.SwitchTrayIcon(Settings.StartMinimizedToTray);
        }

        private void InitializeDisplayManager()
        {
            DisplayManager.Instance.LoadSettings(Settings);
            DisplayManager.HDRIsActiveChanged += DisplayManager_HDRIsActiveChanged;
            HDRIsActive = DisplayManager.GlobalHDRIsActive;

        }
        private void CreateRelayCommands()
        {
            ActivateHDRCommand = new RelayCommand(DisplayManager.Instance.ActivateHDR);
            DeactivateHDRCommand = new RelayCommand(DisplayManager.Instance.DeactivateHDR);
            AddAssignmentCommand = new RelayCommand(AddAssignment);
            RemoveAssignmentCommand = new RelayCommand<ApplicationProfileAssignment>(RemoveAssignment);

            MoveAssignmentUpCommand = new RelayCommand<ApplicationProfileAssignment>(MoveAssignmentUp);
            MoveAssignmentDownCommand = new RelayCommand<ApplicationProfileAssignment>(MoveAssignmentDown);

            AddProfileCommand = new RelayCommand(AddProfile);
            RemoveProfileCommand = new RelayCommand<Profile>(RemoveProfile);

            ClosingCommand = new RelayCommand(Closing);
            ShutdownCommand = new RelayCommand(Shutdown);
            StartApplicationCommand = new RelayCommand<ApplicationItem>(StartApplication);
            ShowInfoCommand = new RelayCommand(ShowInfo);
            BuyBeerCommand = new RelayCommand(BuyBeer);
        }




        #endregion Initialization


        private void DisplayManager_HDRIsActiveChanged(object sender, EventArgs e)
        {
            HDRIsActive = DisplayManager.GlobalHDRIsActive;
        }


        private void TrayMenuHelper_OpenViewRequested(object sender, EventArgs e)
        {
            ShowView = true;
        }

        private void TrayMenuHelper_CloseApplicationRequested(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void LoadSettings()
        {
            Globals.Instance.LoadSettings();

            Settings.ApplicationProfileAssignments.CollectionChanged += ApplicationProfileAssigments_CollectionChanged;
            Settings.ApplicationProfiles.CollectionChanged += ApplicationProfiles_CollectionChanged;
            Settings.Monitors.CollectionChanged += Monitors_CollectionChanged;

            Settings.PropertyChanged += Settings_PropertyChanged;

            ApplicationProfileAssigments_CollectionChanged(
            Settings.ApplicationProfileAssignments, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Settings.ApplicationProfileAssignments.ToList()));

            ApplicationProfiles_CollectionChanged( Settings.ApplicationProfiles, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Settings.ApplicationProfiles.ToList()));

            Monitors_CollectionChanged( Settings.Monitors, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Settings.Monitors.ToList()));


            Logs.LoggingEnabled = Settings.Logging;
            Tools.Logs.Add("Settings loaded", false);
        }


        private void TrayMenu_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Tools.Logs.Add("Open app from Tray", false);
            ShowView = true;

        }


        private void StartApplication(ApplicationItem application)
        {
            Tools.Logs.Add($"Start application {application.ApplicationName}", false);
            try
            {
                DisplayManager.Instance.ActivateHDR();
                System.Threading.Thread.Sleep(2500);
                application.StartApplication();
               
            }
            catch (Exception ex)
            {
                Tools.Logs.AddException(ex);
            }
        }

        private void Closing()
        {
            if (Settings.CloseToTray)
            {
                Tools.Logs.Add($"Minimizing to tray...", false);
                //  TrayMenuHelper.SwitchTrayIcon(true);
            }
            else
            {
                TrayMenuHelper.SwitchTrayIcon(false);

                Tools.Logs.Add($"Shutting down...", false);
                Shutdown();
            }
        }

        private void Shutdown()
        {
            Tools.Logs.Add($"Stopping application watcher...", false);
            ApplicationWatcher.NewLog -= ApplicationWatcher_NewLog;
            ApplicationWatcher.ApplicationChanged -= ApplicationWatcher_ApplicationChanged;
            ApplicationWatcher.Stop();
            Stop();
            //  TrayMenuHelper.SwitchTrayIcon(false);
            Application.Current.Shutdown();
        }

        public void Start()
        {
            lock (_accessLock)
            {
                if (Started)
                    return;
                Tools.Logs.Add($"Starting HDR Monitoring...", false);
                DisplayManager.Instance.StartMonitoring();
                Tools.Logs.Add($"HDR Monitoring started", false);
                Started = true;
               // UpdateHDRModeBasedOnCurrentApplication();
            }
        }

        public void Stop()
        {
            lock (_accessLock)
            {
                if (!Started)
                    return;
                Tools.Logs.Add($"Stopping HDR Monitoring...", false);
                DisplayManager.Instance.StopMonitoring();
                Tools.Logs.Add($"HDR Monitoring stopped", false);
                Started = false;
                Tools.Logs.Add($"Process watcher stopped", false);

            }

        }


        #region Process handling


        private void AddAssignment()
        {
            ApplicationAdder adder = new ApplicationAdder();
            adder.DialogService = DialogService;
            adder.OKClicked += (o, e) =>
            {
                if (!Settings.ApplicationProfileAssignments.Any(pi => pi.Application.ApplicationFilePath == adder.ApplicationItem.ApplicationFilePath))
                {
                    ApplicationProfileAssignment.NewAssigment(adder.ApplicationItem);
                }
            };
            if (DialogService != null)
                DialogService.ShowDialogModal(adder, new System.Drawing.Size(800, 600));
        }


        private void RemoveAssignment(ApplicationProfileAssignment assignment)
        {
            Settings.ApplicationProfileAssignments.Remove(assignment);

        }

        private void MoveAssignmentDown(ApplicationProfileAssignment assignment)
        {
            assignment.ChangePosition(false);
        }

        private void MoveAssignmentUp(ApplicationProfileAssignment assignment)
        {
            assignment.ChangePosition(true);
        }

        private void AddProfile()
        {
            int count = 0;
            string profileName = string.Empty;
            while (string.IsNullOrEmpty(profileName) ||  Settings.ApplicationProfiles.Any(p => p.Name.ToUpperInvariant().Equals(profileName.ToUpperInvariant())))
            {
                count++;
                profileName = $"{ProjectResources.Locale_Texts.Profile} {count}";
            }
            Settings.ApplicationProfiles.Add(new Profile() { Name = profileName });
        }


        private void RemoveProfile(Profile profile)
        {
            Settings.ApplicationProfiles.Remove(profile);
        }

        #endregion Process handling 


        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (_accessLock)
            {
                try
                {
                    Tools.SetAutoStart(ProjectResources.Locale_Texts.AutoHDR, System.Reflection.Assembly.GetEntryAssembly().Location, Settings.AutoStart);

                }
                catch (Exception ex)
                {
                    Tools.Logs.AddException(ex);
                }
                Logs.LoggingEnabled = Settings.Logging;
                Globals.Instance.SaveSettings();
            }
        }


        private void Monitors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    foreach (Display display in e.NewItems)
                    {
                        Tools.Logs.Add($"Display added: {display.Name}", false);
                        display.PropertyChanged += Monitor_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Display display in e.OldItems)
                    {
                        Tools.Logs.Add($"Display removed: {display.Name}", false);
                        display.PropertyChanged += Monitor_PropertyChanged;
                    }
                    break;
            }

            Globals.Instance.SaveSettings();
        }

        private void Monitor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Display.Managed))
                Globals.Instance.SaveSettings();
        }
        private void ApplicationProfiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            switch (e.Action)
            {

                case NotifyCollectionChangedAction.Add:
                    foreach (Profile profile in e.NewItems)
                    {
                        Tools.Logs.Add($"Profile added: {profile.Name}", false);
                        profile.ApplicationClosed.CollectionChanged += ProfileActions_CollectionChanged;
                        profile.ApplicationStarted.CollectionChanged += ProfileActions_CollectionChanged;
                        profile.ApplicationLostFocus.CollectionChanged += ProfileActions_CollectionChanged;
                        profile.ApplicationGotFocus.CollectionChanged += ProfileActions_CollectionChanged;
                        profile.PropertyChanged += SaveSettingsOnPropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Profile profile in e.OldItems)
                    {
                        Tools.Logs.Add($"Profile removed: {profile.Name}", false);
                        profile.ApplicationClosed.CollectionChanged -= ProfileActions_CollectionChanged;
                        profile.ApplicationStarted.CollectionChanged -= ProfileActions_CollectionChanged;
                        profile.ApplicationLostFocus.CollectionChanged -= ProfileActions_CollectionChanged;
                        profile.ApplicationGotFocus.CollectionChanged -= ProfileActions_CollectionChanged;
                        profile.PropertyChanged -= SaveSettingsOnPropertyChanged;
                    }
                    break;
            }
            Globals.Instance.SaveSettings();

        }


        private void ProfileActions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Globals.Instance.SaveSettings();
        }

        private void ApplicationProfileAssigments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    foreach (ApplicationProfileAssignment assignment in e.NewItems)
                    {


                        Tools.Logs.Add($"Application added: {assignment.Application.ApplicationName}", false);
                        assignment.PropertyChanged += SaveSettingsOnPropertyChanged;
                        ApplicationWatcher.AddProcess(assignment.Application);
                        assignment.Application.PropertyChanged += SaveSettingsOnPropertyChanged;
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ApplicationProfileAssignment assignment in e.OldItems)
                    {
                        Tools.Logs.Add($"Application removed: {assignment.Application.ApplicationName}", false);
                        assignment.PropertyChanged -= SaveSettingsOnPropertyChanged;

                        ApplicationWatcher.RemoveProcess(assignment.Application);
                        assignment.Application.PropertyChanged -= SaveSettingsOnPropertyChanged;
                    }

                    break;

            }
            Globals.Instance.SaveSettings();
        }
       


        private void SaveSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Globals.Instance.SaveSettings();
        }



        private void ShowInfo()
        {
            ShowInfo(null);
        }

        private void ShowInfo(GitHubData data)
        {
            AutoHDRInfo info;
            if (data == null)
                info = new AutoHDRInfo();
            else
                info = new AutoHDRInfo(data);
            if (DialogService != null)
                DialogService.ShowDialogModal(info, new System.Drawing.Size(600, 1000));
        }

        private void BuyBeer()
        {
            Process.Start(new ProcessStartInfo((string)Application.Current.Resources["DonateLink"]));
        }
    }
}
