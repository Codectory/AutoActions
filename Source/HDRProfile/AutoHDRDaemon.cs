using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using Hardcodet.Wpf.TaskbarNotification;
using AutoHDR.Displays;
using AutoHDR.Info;
using AutoHDR.Info.Github;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoHDR
{
    public class AutoHDRDaemon : BaseViewModel
    {
        readonly object _accessLock = new object();
        private bool _showView = false;
        private ApplicationItem _currentApplication = null;

        private bool _hdrIsActive;
        private UserAppSettings _settings;
        private DisplayManager _monitorManager;

        Dictionary<ApplicationItem, ApplicationState> _lastAppStates = new Dictionary<ApplicationItem, ApplicationState>();

        private string SettingsPathCompatible => $"{System.AppDomain.CurrentDomain.BaseDirectory}HDRProfile_Settings.xml";

        private string SettingsPath => $"{System.AppDomain.CurrentDomain.BaseDirectory}UserSettings.xml";

        private bool started = false;
        public bool Started { get => started; private set { started = value; OnPropertyChanged(); } }
        ProcessWatcher ProcessWatcher;
        TrayMenuHelper TrayMenuHelper;


        #region RelayCommands

        public RelayCommand ActivateHDRCommand { get; private set; }
        public RelayCommand DeactivateHDRCommand { get; private set; }
        public RelayCommand AddApplicationCommand { get; private set; }
        public RelayCommand ShowInfoCommand { get; private set; }

        public RelayCommand<ApplicationItem> RemoveApplicationCommand { get; private set; }
        public RelayCommand LoadingCommand { get; private set; }
        public RelayCommand ClosingCommand { get; private set; }
        public RelayCommand ShutdownCommand { get; private set; }
        public RelayCommand BuyBeerCommand { get; private set; }

        public RelayCommand<ApplicationItem> StartApplicationCommand { get; private set; }

        #endregion RelayCommands

        public UserAppSettings Settings { get => _settings; set { _settings = value; OnPropertyChanged(); } }
        public DisplayManager MonitorManager { get => _monitorManager; set { _monitorManager = value; OnPropertyChanged(); } }

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
        /// <summary>
        /// Returns true if a Application with preventHDR Option is running or focused
        /// </summary>
        public bool IsHDRAllowedToActivate()
        {
            return !ProcessWatcher.Applications.Any(e => (e.Value == ApplicationState.Running || e.Value == ApplicationState.Focused) && e.Key.PreventHdr == true);
        }

        public AutoHDRDaemon()
        {
            Logs.LoggingEnabled = true;
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
                Tools.Logs.Add("Initializing...", false);
                ProcessWatcher = new ProcessWatcher();
                ProcessWatcher.OneProcessIsRunningChanged += ProcessWatcher_RunningOrFocusedChanged;
                ProcessWatcher.OneProcessIsFocusedChanged += ProcessWatcher_RunningOrFocusedChanged;

                LoadSettings();
                if (Settings.CheckForNewVersion)
                    CheckForNewVersion();
                InitializeMonitorManager();
                SaveSettings();
                InitializeTrayMenuHelper();
                CreateRelayCommands();
                ShowView = !Settings.StartMinimizedToTray;
                Initialized = true;
                Tools.Logs.Add("Initialized", false);
                Start();
                Tools.Logs.Add($"Starting process watcher...", false);
                ProcessWatcher.Start();
                Tools.Logs.Add($"Process watcher started", false);

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
            TrayMenuHelper.Initialize(MonitorManager);
            TrayMenuHelper.OpenViewRequested += TrayMenuHelper_OpenViewRequested;
            TrayMenuHelper.CloseApplicationRequested += TrayMenuHelper_CloseApplicationRequested;
            //TrayMenuHelper.SwitchTrayIcon(Settings.StartMinimizedToTray);
        }

        private void InitializeMonitorManager()
        {
            MonitorManager = new DisplayManager(Settings);
            DisplayManager.HDRIsActiveChanged += DisplayManager_HDRIsActiveChanged;
            MonitorManager.AutoHDRChanged += DisplayManager_AutoHDRChanged;
            HDRIsActive = DisplayManager.GlobalHDRIsActive;

        }
        private void CreateRelayCommands()
        {
            ActivateHDRCommand = new RelayCommand(MonitorManager.ActivateHDR);
            DeactivateHDRCommand = new RelayCommand(MonitorManager.DeactivateHDR);
            AddApplicationCommand = new RelayCommand(AddAplication);
            RemoveApplicationCommand = new RelayCommand<ApplicationItem>(RemoveApplication);
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

        private void DisplayManager_AutoHDRChanged(object sender, EventArgs e)
        {
            SaveSettings();
            UpdateHDRModeBasedOnCurrentApplication();
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
                File.Move(SettingsPath, backupFile);
                Tools.Logs.Add($"Created backup of invalid settings file: {backupFile}", false);
                File.Delete(SettingsPath);
                Tools.Logs.Add("Failed to load settings", false);
                Tools.Logs.AddException(ex);
                Settings = new UserAppSettings();
                SaveSettings();
                Tools.Logs.Add("Created new settings file", false);
            }

            Settings.ApplicationItems.CollectionChanged += ApplicationItems_CollectionChanged;
            _settings.PropertyChanged += Settings_PropertyChanged;
            Logs.LoggingEnabled = Settings.Logging;
            foreach (var application in Settings.ApplicationItems)
            {
                ProcessWatcher.AddProcess(application);
                application.PropertyChanged += ApplicationItem_PropertyChanged;
            }
            Tools.Logs.Add("Settings loaded", false);
        }




        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (_accessLock)
            {
                try
                {
                    Tools.SetAutoStart(ProjectResources.Locale_Texts.AutoHDR, System.Reflection.Assembly.GetEntryAssembly().Location, _settings.AutoStart);

                }
                catch (Exception ex)
                {
                    Tools.Logs.AddException(ex);
                }
                if (e.PropertyName.Equals(nameof(Settings.HDRMode)))
                    UpdateHDRModeBasedOnCurrentApplication();
                Logs.LoggingEnabled = Settings.Logging;
                SaveSettings();
            }
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
                MonitorManager.ActivateHDR();
                System.Threading.Thread.Sleep(2500);
                if (application.IsUWP)
                {
                    UWP.UWPAppsManager.StartUWPApp(application.UWPFamilyPackageName, application.UWPApplicationID);
                }
                else
                {
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo(application.ApplicationFilePath);
                    process.Start();
                }
                System.Threading.Thread.Sleep(2500);
                var processes = Process.GetProcessesByName(application.ApplicationName).ToList();
                if (processes.Count > 0)
                {
                    Process foundProcess = new Process();
                    Tools.Logs.Add($"Bring application to front: {application.ApplicationName}", false);
                    foundProcess = processes[0];
                    if(!foundProcess.HasExited && foundProcess.Responding)
                    Tools.BringMainWindowToFront(foundProcess.ProcessName);
                }
                else
                    Tools.Logs.Add($"No started application found: {application.ApplicationName}", false);

            }
            catch (Exception ex)
            {
                try
                {
                    //can throw an error
                    Tools.Logs.AddException(ex);
                }
                catch { }
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
            Tools.Logs.Add($"Stopping process watcher...", false);
            ProcessWatcher.OneProcessIsRunningChanged -= ProcessWatcher_RunningOrFocusedChanged;
            ProcessWatcher.OneProcessIsFocusedChanged -= ProcessWatcher_RunningOrFocusedChanged;
            ProcessWatcher.Stop();
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
                MonitorManager.StartMonitoring();
                Tools.Logs.Add($"HDR Monitoring started", false);
                Started = true;
                UpdateHDRModeBasedOnCurrentApplication();
            }
        }

        public void Stop()
        {
            lock (_accessLock)
            {
                if (!Started)
                    return;
                Tools.Logs.Add($"Stopping HDR Monitoring...", false);
                MonitorManager.StopMonitoring();
                Tools.Logs.Add($"HDR Monitoring stopped", false);
                Started = false;
                Tools.Logs.Add($"Process watcher stopped", false);

            }

        }


        #region Process handling

        private void ApplicationItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (_accessLock)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var applicationItem in e.NewItems)
                        {
                            Tools.Logs.Add($"Application added: {((ApplicationItem)applicationItem).ApplicationName}", false);
                            ProcessWatcher.AddProcess(((ApplicationItem)applicationItem));
                            ((ApplicationItem)applicationItem).PropertyChanged += ApplicationItem_PropertyChanged;
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var applicationItem in e.OldItems)
                        {
                            Tools.Logs.Add($"Application removed: {((ApplicationItem)applicationItem).ApplicationName}", false);
                            ProcessWatcher.RemoveProcess(((ApplicationItem)applicationItem));
                            ((ApplicationItem)applicationItem).PropertyChanged -= ApplicationItem_PropertyChanged;

                        }
                        break;

                }

                SaveSettings();
            }
        }

        private void SaveSettings()
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

        private void ApplicationItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSettings();
        }

        private void AddAplication()
        {
            ApplicationAdder adder = new ApplicationAdder();
            adder.DialogService = DialogService;
            adder.OKClicked += (o, e) =>
            {
                if (!Settings.ApplicationItems.Any(pi => pi.ApplicationFilePath == adder.ApplicationItem.ApplicationFilePath))
                    Settings.ApplicationItems.Add(adder.ApplicationItem);
            };
            if (DialogService != null)
                DialogService.ShowDialogModal(adder);
        }


        private void RemoveApplication(ApplicationItem process)
        {
            Settings.ApplicationItems.Remove(process);

        }


        private void ProcessWatcher_RunningOrFocusedChanged(object sender, EventArgs e)
        {
            CurrentApplication = ProcessWatcher.CurrentRunningApplicationItem;
            UpdateHDRModeBasedOnCurrentApplication();

        }
        /// <summary>
        /// Converts OnOff Enum to boolean
        /// </summary>
        /// <param name="OnOff"></param>
        /// <returns></returns>
        private bool OnOffToBool(OnOff OnOff)
        {
            switch (OnOff){
                case OnOff
            .ON: return true;
                default: return false;
            }
        }
        private void UpdateHDRModeBasedOnCurrentApplication()
        {
            lock (_accessLock)
            {
                try {
                   
                //check if one running process is preventing hdr
                    bool allowedHDR = IsHDRAllowedToActivate();

                    //check running processes if we should activate hdr
                    bool processRequestHDR = false;
                    switch (Settings.HDRMode)
                    {
                        case HDRActivationMode.Running:
                            processRequestHDR = ProcessWatcher.OneProcessIsRunning;
                            break;
                        case HDRActivationMode.Focused:
                            processRequestHDR = ProcessWatcher.OneProcessIsFocused;
                            break;
                        default:
                            return;
                    }
                    bool activateHDR = false;
                    if (allowedHDR)
                    {
                        if (OnOffToBool(Settings.DesktopHDRDefault))
                        {
                            //hdr is always active on desktop default and allowedHdr
                            activateHDR = true;
                        }
                        else
                        {
                            //activateHdr  is dependend on process if desktop is not default hdr
                            activateHDR = processRequestHDR;
                        }
                    }


                    //hdr is already active)
                    if (activateHDR == HDRIsActive)
                    {
                        return;
                    }
                    //hdr hast to be activated
                    if (activateHDR)
                    {
                        Tools.Logs.Add($"Activating HDR...", false);
                        MonitorManager.ActivateHDR();

                    }
                    //hdr has to be deactivated
                    else if (DisplayManager.GlobalHDRIsActive && Settings.HDRMode != HDRActivationMode.None)
                    {
                        Tools.Logs.Add($"Deactivating HDR...", false);
                        if (Settings.GlobalAutoHDR)
                            MonitorManager.DeactivateHDR();
                        else
                        {
                            foreach (Display display in Settings.Monitors)
                                if (display.Managed)
                                    DisplayManager.DeactivateHDR(display);
                        }
                    }
                    var currentApplications = ProcessWatcher.Applications;
                    UpdateRestartAppStates((IDictionary<ApplicationItem, ApplicationState>)currentApplications, activateHDR);

                    if (DisplayManager.GlobalHDRIsActive)
                        Tools.Logs.Add($"HDR is active", false);
                    else
                        Tools.Logs.Add($"HDR is inactive", false);
                }
                catch (Exception ex)
                {
                    Tools.Logs.AddException(ex);
                    throw ex;
                }
            }
        }

        private void UpdateRestartAppStates(IDictionary<ApplicationItem, ApplicationState> applicationStates, bool restartApps)
        {
            Dictionary<ApplicationItem, ApplicationState> newLastAppStates = new Dictionary<ApplicationItem, ApplicationState>();
            Tools.Logs.Add($"Updating application states...", false);
            foreach (var applicationState in applicationStates)
            {
                newLastAppStates.Add(applicationState.Key, applicationState.Value);

                if (applicationState.Key.RestartProcess &&  restartApps)
                {
                    if (!_lastAppStates.ContainsKey(applicationState.Key) && applicationState.Value != ApplicationState.None)
                        RestartProcess(applicationState.Key);
                    else if (_lastAppStates.ContainsKey(applicationState.Key) && applicationState.Value != ApplicationState.None && _lastAppStates[applicationState.Key] == ApplicationState.None)
                        RestartProcess(applicationState.Key);
                }
            }
            _lastAppStates.Clear();
            _lastAppStates = newLastAppStates;
        }

        private void RestartProcess(ApplicationItem application)
        {
            try
            {
                Tools.Logs.Add($"Restarting application {application.ApplicationName}", false);
                foreach (Process process in Process.GetProcessesByName(application.ApplicationName).ToList())
                    if (process.StartTime < Process.GetCurrentProcess().StartTime)
                    {
                        Tools.Logs.Add($"Won't restart application {application.ApplicationName} as it was running before { ProjectResources.Locale_Texts.AutoHDR}.", false);

                        return;
                    }
                Process.GetProcessesByName(application.ApplicationName).ToList().ForEach(p => p.Kill());
                System.Threading.Thread.Sleep(1500);
                StartApplication(application);
            }
            catch (Exception ex)
            {
                Tools.Logs.AddException($"Failed to restart process {application.DisplayName} ({application.ApplicationFilePath}).", ex);
                throw;
            }
        }

        #endregion Process handling 


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
                DialogService.ShowDialogModal(info, new System.Drawing.Size(450,650));
        }

        private void BuyBeer()
        {
            Process.Start(new ProcessStartInfo((string)Application.Current.Resources["DonateLink"]));
        }
    }
}
