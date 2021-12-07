using AutoHDR.Audio;
using AutoHDR.Displays;
using AutoHDR.Info;
using AutoHDR.Info.Github;
using AutoHDR.Profiles;
using AutoHDR.Profiles.Actions;
using CodectoryCore;
using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using CodectoryCore.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<IProfileAction> _lastActions;


        private bool _hdrIsActive;


        private bool started = false;
        public bool Started { get => started; private set { started = value; OnPropertyChanged(); } }
        ProcessWatcher ApplicationWatcher;
        TrayMenuHelper TrayMenuHelper;

        LogsStorage _logsStorage;


        #region RelayCommands

        public RelayCommand ActivateHDRCommand { get; private set; }
        public RelayCommand DeactivateHDRCommand { get; private set; }
        public RelayCommand AddAssignmentCommand { get; private set; }
        public RelayCommand<ApplicationProfileAssignment> EditApplicationCommand { get; private set; }

        public RelayCommand<ApplicationProfileAssignment> RemoveAssignmentCommand { get; private set; }

        public RelayCommand<ApplicationProfileAssignment> MoveAssignmentUpCommand { get; private set; }
        public RelayCommand<ApplicationProfileAssignment> MoveAssignmentDownCommand { get; private set; }

        public RelayCommand AddProfileCommand { get; private set; }
        public RelayCommand<Profile> RemoveProfileCommand { get; private set; }
        public RelayCommand ShowInfoCommand { get; private set; }
        public RelayCommand ShowLogsCommand { get; private set; }
        public RelayCommand ShowLicenseCommand { get; private set; }


        public RelayCommand LoadingCommand { get; private set; }
        public RelayCommand ClosingCommand { get; private set; }
        public RelayCommand ShutdownCommand { get; private set; }
        public RelayCommand BuyBeerCommand { get; private set; }

        public RelayCommand<ApplicationItem> StartApplicationCommand { get; private set; }

        #endregion RelayCommands

        public UserAppSettings Settings { get => Globals.Instance.Settings; set { Globals.Instance.Settings = value; OnPropertyChanged(); } }
        public Profile CurrentProfile { get => _currentProfile; set { _currentProfile = value; OnPropertyChanged(); } }
        public ObservableCollection<IProfileAction> LastActions { get => _lastActions; set { _lastActions = value; OnPropertyChanged(); } }



        public bool Initialized { get; private set; } = false;
        public bool ShowView { get => _showView; set { _showView = value; OnPropertyChanged(); } }

        public ApplicationItem CurrentApplication { get => _currentApplication; set { _currentApplication = value; OnPropertyChanged(); } }

        public bool HDRIsActive { get => _hdrIsActive; set { _hdrIsActive = value; OnPropertyChanged(); } }
        public Version Version
        {
            get
            {
                return VersionExtension.ApplicationVersion(System.Reflection.Assembly.GetExecutingAssembly());
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
                _logsStorage = new LogsStorage();
                _lastActions = new ObservableCollection<IProfileAction>();
                ApplicationWatcher = new ProcessWatcher();
                ApplicationWatcher.NewLog += ApplicationWatcher_NewLog;
                ApplicationWatcher.ApplicationChanged += ApplicationWatcher_ApplicationChanged;
                LoadSettings();
                Globals.Logs.Add("Initializing...", false);

                if (Settings.CheckForNewVersion)
                    CheckForNewVersion();
                InitializeDisplayManager();
                InitializeAudioManager();
                Globals.Instance.SaveSettings();
                InitializeTrayMenuHelper();
                CreateRelayCommands();
                ShowView = !Settings.StartMinimizedToTray;
                Initialized = true;
                Globals.Logs.Add("Initialized", false);
                Start();
                ApplicationWatcher.Start();

            }
        }

        private void ApplicationWatcher_ApplicationChanged(object sender, ApplicationChangedEventArgs e)
        {
            Globals.Logs.Add($"Application {e.Application} changed: {e.ChangedType}", false);
            CurrentApplication = e.Application;
            UpdateCurrentProfile(e.Application, e.ChangedType);
            if (e.ChangedType == ApplicationChangedType.Closed)
                CurrentApplication = null;
        }

        private void ApplicationWatcher_NewLog(object sender, string e)
        {
            Globals.Logs.Add(e, false);
        }

        private void UpdateCurrentProfile(ApplicationItem application, ApplicationChangedType changedType)
        {
            lock (_accessLock)
            {
                ApplicationProfileAssignment assignment = Settings.ApplicationProfileAssignments.First(a => a.Application.ApplicationFilePath.Equals(application.ApplicationFilePath));

                if (assignment == null)
                {
                    Globals.Logs.Add($"No assignmet for {application.ApplicationFilePath}.", false);
                    CurrentProfile = null;
                    return;
                }
                Profile profile = assignment.Profile;


                if (profile == null)
                    return;
                bool profileChanged = Equals(profile, CurrentProfile);

                CurrentProfile = profile;
                if (profileChanged)
                    Globals.Logs.Add($"Profile changed to {profile.Name}", false);
                List<IProfileAction> actions = new List<IProfileAction>();
                switch (changedType)
                {
                    case ApplicationChangedType.Started:
                        actions = profile.ApplicationStarted.ToList();
                        break;
                    case ApplicationChangedType.Closed:
                        actions = profile.ApplicationClosed.ToList();
                        break;
                    case ApplicationChangedType.GotFocus:
                        actions = profile.ApplicationGotFocus.ToList();
                        break;
                    case ApplicationChangedType.LostFocus:
                        actions = profile.ApplicationLostFocus.ToList();
                        break;
                }
                if (actions.Count > 0)
                    App.Current.Dispatcher.Invoke(() => LastActions.Clear());
                foreach (var action in actions)
                {
                    App.Current.Dispatcher.Invoke(() => LastActions.Add(action));
                    action.NewLog += ActionLog;
                    action.RunAction();
                    action.NewLog -= ActionLog;
                    System.Threading.Thread.Sleep(100);
                }
                if (profile.RestartApplication && changedType == ApplicationChangedType.Started)
                    assignment.Application.Restart();
                if (changedType == ApplicationChangedType.Closed)
                    CurrentProfile = null;
            }
        }

        private void ActionLog(object sender, LogEntry entry)
        {
            Globals.Logs.AppendLogEntry(entry);
        }

        private void CheckForNewVersion()
        {
            Task.Run(() =>
            {
                Globals.Logs.Add($"Checking for new version...", false);

                GitHubData data = GitHubIntegration.GetGitHubData();
                Version localVersion = VersionExtension.ApplicationVersion(System.Reflection.Assembly.GetExecutingAssembly());
                int versionComparison = localVersion.CompareTo(data.CurrentVersion);
                if (versionComparison < 0)
                {
                    Globals.Logs.Add($"Newer version availabe.", false);

                    Application.Current.Dispatcher.Invoke(
                      (Action)(() =>
                      {
                          ShowInfo(data);
                      }));
                }
                else
                    Globals.Logs.Add($"Local version is up to date.", false);
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

        private void InitializeAudioManager()
        {
            AudioManager.Initialize();
        }

        private void CreateRelayCommands()
        {
            ActivateHDRCommand = new RelayCommand(DisplayManager.Instance.ActivateHDR);
            DeactivateHDRCommand = new RelayCommand(DisplayManager.Instance.DeactivateHDR);
            AddAssignmentCommand = new RelayCommand(AddAssignment);
            EditApplicationCommand = new RelayCommand<ApplicationProfileAssignment>(EditApplication);
            RemoveAssignmentCommand = new RelayCommand<ApplicationProfileAssignment>(RemoveAssignment);

            MoveAssignmentUpCommand = new RelayCommand<ApplicationProfileAssignment>(MoveAssignmentUp);
            MoveAssignmentDownCommand = new RelayCommand<ApplicationProfileAssignment>(MoveAssignmentDown);

            AddProfileCommand = new RelayCommand(AddProfile);
            RemoveProfileCommand = new RelayCommand<Profile>(RemoveProfile);

            ClosingCommand = new RelayCommand(Closing);
            ShutdownCommand = new RelayCommand(Shutdown);
            StartApplicationCommand = new RelayCommand<ApplicationItem>(StartApplication);
            ShowLicenseCommand = new RelayCommand(ShowLicense);
            ShowInfoCommand = new RelayCommand(ShowInfo);
            ShowLogsCommand = new RelayCommand(ShowLogs);

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
            FixAssignments();
            Globals.Instance.SaveSettings();
            Settings.ApplicationProfileAssignments.Sort(a => a.Position, ListSortDirection.Ascending);
            Settings.ApplicationProfileAssignments.CollectionChanged += ApplicationProfileAssigments_CollectionChanged;
            Settings.ApplicationProfiles.CollectionChanged += ApplicationProfiles_CollectionChanged;
            Settings.Monitors.CollectionChanged += Monitors_CollectionChanged;

            Settings.PropertyChanged += Settings_PropertyChanged;

            ApplicationProfileAssigments_CollectionChanged(
            Settings.ApplicationProfileAssignments, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Settings.ApplicationProfileAssignments.ToList()));

            ApplicationProfiles_CollectionChanged(Settings.ApplicationProfiles, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Settings.ApplicationProfiles.ToList()));

            Monitors_CollectionChanged(Settings.Monitors, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Settings.Monitors.ToList()));


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


        private void TrayMenu_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Globals.Logs.Add("Open app from Tray", false);
            ShowView = true;

        }


        private void StartApplication(ApplicationItem application)
        {
            Globals.Logs.Add($"Start application {application.ApplicationName}", false);
            try
            {
                DisplayManager.Instance.ActivateHDR();
                System.Threading.Thread.Sleep(2500);
                application.StartApplication();

            }
            catch (Exception ex)
            {
                Globals.Logs.AddException(ex);
            }
        }

        private void Closing()
        {
            if (Settings.CloseToTray)
            {
                Globals.Logs.Add($"Minimizing to tray...", false);
                //  TrayMenuHelper.SwitchTrayIcon(true);
            }
            else
            {
                TrayMenuHelper.SwitchTrayIcon(false);

                Globals.Logs.Add($"Shutting down...", false);
                Shutdown();
            }
        }

        private void Shutdown()
        {
            Globals.Logs.Add($"Stopping application watcher...", false);
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
                Globals.Logs.Add($"Starting HDR Monitoring...", false);
                DisplayManager.Instance.StartMonitoring();
                Globals.Logs.Add($"HDR Monitoring started", false);
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
                Globals.Logs.Add($"Stopping HDR Monitoring...", false);
                DisplayManager.Instance.StopMonitoring();
                Globals.Logs.Add($"HDR Monitoring stopped", false);
                Started = false;
                Globals.Logs.Add($"Process watcher stopped", false);

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
                   var assignment = ApplicationProfileAssignment.NewAssigment(adder.ApplicationItem);
                    if (Settings.DefaultProfile != null)
                        assignment.Profile = Settings.DefaultProfile;
                }
                Settings.ApplicationProfileAssignments.Sort(x => x.Position, System.ComponentModel.ListSortDirection.Ascending);

            };
            if (DialogService != null)
                DialogService.ShowDialogModal(adder, new System.Drawing.Size(800, 600));
        }


        private void EditApplication(ApplicationProfileAssignment assignment)
        {


            ApplicationAdder adder = new ApplicationAdder(assignment.Application);
            adder.DialogService = DialogService;
            adder.OKClicked += (o, e) =>
            {
                assignment.Application = adder.ApplicationItem;
            };
            if (DialogService != null)
                DialogService.ShowDialogModal(adder, new System.Drawing.Size(800, 600));
        }

        private void RemoveAssignment(ApplicationProfileAssignment assignment)
        {
            Settings.ApplicationProfileAssignments.Remove(assignment);
            Settings.ApplicationProfileAssignments.Sort(x => x.Position, System.ComponentModel.ListSortDirection.Ascending);

        }

        private void MoveAssignmentDown(ApplicationProfileAssignment assignment)
        {
            if (assignment.Position == Settings.ApplicationProfileAssignments.Count - 1)
                return;
            Settings.ApplicationProfileAssignments.Move(assignment.Position, assignment.Position + 1);

        }

        private void MoveAssignmentUp(ApplicationProfileAssignment assignment)
        {
            if (assignment.Position == 0)
                return;
            Settings.ApplicationProfileAssignments.Move(assignment.Position, assignment.Position - 1);

        }

        private void AddProfile()
        {
            lock (_accessLock)
            {
                int count = 0;
                string profileName = string.Empty;
                while (string.IsNullOrEmpty(profileName) || Settings.ApplicationProfiles.Any(p => p.Name.ToUpperInvariant().Equals(profileName.ToUpperInvariant())))
                {
                    count++;
                    profileName = $"{ProjectResources.Locale_Texts.Profile} {count}";
                }
                Settings.ApplicationProfiles.Add(new Profile() { Name = profileName });
            }
        }


        private void RemoveProfile(Profile profile)
        {
            lock (_accessLock)
            {
                Settings.ApplicationProfiles.Remove(profile);
            }
        }

        #endregion Process handling 


        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (_accessLock)
            {
                try
                {
                    if (Settings.AutoStart)
                        AutoStart.Activate(ProjectResources.Locale_Texts.AutoHDR, System.Reflection.Assembly.GetEntryAssembly().Location);
                    else
                        AutoStart.Deactivate(ProjectResources.Locale_Texts.AutoHDR, System.Reflection.Assembly.GetEntryAssembly().Location);
                }
                catch (Exception ex)
                {
                    Globals.Logs.AddException(ex);
                }
                Globals.Logs.LogFileEnabled = Settings.CreateLogFile;
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
                        Globals.Logs.Add($"Display added: {display.Name}", false);
                        display.PropertyChanged += Monitor_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Display display in e.OldItems)
                    {
                        Globals.Logs.Add($"Display removed: {display.Name}", false);
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
                        Globals.Logs.Add($"Profile added: {profile.Name}", false);
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
                        Globals.Logs.Add($"Profile removed: {profile.Name}", false);
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

        readonly object _lockAssignments = new object();

        private void ApplicationProfileAssigments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool taken = Monitor.TryEnter(_lockAssignments);
            if (!taken)
                return;
            try
            {
                SortableObservableCollection<ApplicationProfileAssignment> collection = (SortableObservableCollection<ApplicationProfileAssignment>)sender;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:

                        foreach (ApplicationProfileAssignment assignment in e.NewItems)
                        {


                            Globals.Logs.Add($"Application added: {assignment.Application.ApplicationName}", false);
                            assignment.PropertyChanged += SaveSettingsOnPropertyChanged;
                            ApplicationWatcher.AddProcess(assignment.Application);
                            assignment.Application.PropertyChanged += SaveSettingsOnPropertyChanged;
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (ApplicationProfileAssignment assignment in e.OldItems)
                        {
                            Globals.Logs.Add($"Application removed: {assignment.Application.ApplicationName}", false);
                            assignment.PropertyChanged -= SaveSettingsOnPropertyChanged;


                            int removedPosition = assignment.Position;
                            foreach (ApplicationProfileAssignment a in collection)
                            {
                                if (a.Position >= removedPosition)
                                    a.Position = a.Position - 1;
                            }
                            ApplicationWatcher.RemoveProcess(assignment.Application);
                            assignment.Application.PropertyChanged -= SaveSettingsOnPropertyChanged;
                        }

                        break;
                    case NotifyCollectionChangedAction.Move:
                        int downFrom = e.NewStartingIndex;
                        int upFrom = e.OldStartingIndex;

                        if (e.OldStartingIndex == e.NewStartingIndex)
                            break;



                        foreach (ApplicationProfileAssignment assingment in collection)
                        {
                            int position = assingment.Position;
                            if (position == e.OldStartingIndex)
                            {
                                assingment.Position = e.NewStartingIndex;
                            }
                            else if (e.OldStartingIndex > e.NewStartingIndex && position < e.OldStartingIndex && position >= e.NewStartingIndex)
                            {
                                assingment.Position = position + 1;
                            }
                            else if (e.OldStartingIndex < e.NewStartingIndex && position > e.OldStartingIndex && position <= e.NewStartingIndex)
                            {
                                assingment.Position = position - 1;
                            }
                        }

                        break;
                }
                Globals.Instance.SaveSettings();
            }
            finally
            {
                if (taken)
                    Monitor.Exit(_lockAssignments);
            }
        }



        private void SaveSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Globals.Instance.SaveSettings();
        }



        private void ShowInfo()
        {
            ShowInfo(null);
        }

        private void ShowLogs()
        {
            if (DialogService != null)
                DialogService.ShowDialogModal(_logsStorage, new System.Drawing.Size(600, 1000));
        }

        private void ShowLicense()
        {
            if (DialogService != null)
                DialogService.ShowDialogModal(new AutoHDRLicense(), new System.Drawing.Size(600, 1000));
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
