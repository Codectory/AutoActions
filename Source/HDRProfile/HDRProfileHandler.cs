using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using Hardcodet.Wpf.TaskbarNotification;
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

namespace HDRProfile
{
    public class HDRProfileHandler : BaseViewModel
    {
        readonly object _accessLock = new object();
        private bool _showView = false;
        private ApplicationItem _currentApplication = null;

        private bool _hdrIsActive;
        private static Logs Logs = new Logs($"{System.AppDomain.CurrentDomain.BaseDirectory}HDRProfile.log", "HDRPProfile", Assembly.GetExecutingAssembly().GetName().Version.ToString(), true);
        private HDRProfileSettings _settings;
        private MonitorManager _monitorManager;

        Dictionary<ApplicationItem, ApplicationState> _lastRestartAppStates = new Dictionary<ApplicationItem, ApplicationState>();

        private string SettingsPath => $"{System.AppDomain.CurrentDomain.BaseDirectory}HDRProfile_Settings.xml";

        private bool started = false;
        public bool Started { get => started; private set { started = value; OnPropertyChanged(); } }
        ProcessWatcher ProcessWatcher;
        TrayMenuHelper TrayMenuHelper;


        #region RelayCommands

        public RelayCommand ActivateHDRCommand { get; private set; }
        public RelayCommand DeactivateHDRCommand { get; private set; }
        public RelayCommand AddApplicationCommand { get; private set; }
        public RelayCommand<ApplicationItem> RemoveApplicationCommand { get; private set; }
        public RelayCommand LoadingCommand { get; private set; }
        public RelayCommand ClosingCommand { get; private set; }
        public RelayCommand ShutdownCommand { get; private set; }
        public RelayCommand<ApplicationItem> StartApplicationCommand { get; private set; }

        #endregion RelayCommands

        public HDRProfileSettings Settings { get => _settings; set { _settings = value; OnPropertyChanged(); } }
        public MonitorManager MonitorManager { get => _monitorManager; set { _monitorManager = value; OnPropertyChanged(); } }

        public bool Initialized { get; private set; } = false;
        public bool ShowView { get => _showView; set { _showView = value; OnPropertyChanged(); } }

        public ApplicationItem CurrentApplication { get => _currentApplication; set { _currentApplication = value; OnPropertyChanged(); } }

        public bool HDRIsActive { get => _hdrIsActive; set { _hdrIsActive = value; OnPropertyChanged(); } }
        public string Version
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string version = assembly.GetName().Version.ToString();
                version = version.Substring(0, version.LastIndexOf('.'));
                return version;
            }
        }


        public HDRProfileHandler()
        {
            //NightLightManager nightLightManager = new NightLightManager();
            //bool value = nightLightManager.GetNightLightState();
            //nightLightManager.SetNightLightState(false);
            //value = nightLightManager.GetNightLightState();
            //nightLightManager.SetNightLightState(false);

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
                Logs.Add("Initializing...", false);
                ProcessWatcher = new ProcessWatcher();
                ProcessWatcher.OneProcessIsRunningChanged += ProcessWatcher_RunningOrFocusedChanged;
                ProcessWatcher.OneProcessIsFocusedChanged += ProcessWatcher_RunningOrFocusedChanged;

                LoadSettings();
                InitializeMonitorManager();
                SaveSettings();
                InitializeTrayMenuHelper();
                CreateRelayCommands();
                ShowView = !Settings.StartMinimizedToTray;
                Initialized = true;
                Logs.Add("Initialized", false);
                Start();
                Logs.Add($"Starting process watcher...", false);
                ProcessWatcher.Start();
                Logs.Add($"Process watcher started", false);

            }
        }

        private void LogSystem()
        {
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
            MonitorManager = new MonitorManager(Settings);
            MonitorManager.HDRIsActiveChanged += MonitorManager_HDRIsActiveChanged;
            MonitorManager.AutoHDRChanged += MonitorManager_AutoHDRChanged;
            HDRIsActive = MonitorManager.GlobalHDRIsActive;

        }

        private void MonitorManager_HDRIsActiveChanged(object sender, EventArgs e)
        {
            HDRIsActive = MonitorManager.GlobalHDRIsActive;
        }

        private void MonitorManager_AutoHDRChanged(object sender, EventArgs e)
        {
            SaveSettings();
            UpdateHDRBasedOnCurrentApplication();
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
                    Logs.Add("Loading settings...", false);
                    Settings = HDRProfileSettings.ReadSettings(SettingsPath);
                }
                else
                {
                    Logs.Add("Creating settings file", false);

                    Settings = new HDRProfileSettings();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                string backupFile = $"{System.AppDomain.CurrentDomain.BaseDirectory}HDRProfile_Settings_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xml.bak";
                File.Move(SettingsPath, backupFile);
                Logs.Add($"Created backup of invalid settings file: {backupFile}", false);
                File.Delete(SettingsPath);
                Logs.Add("Failed to load settings", false);
                Logs.AddException(ex);
                Settings = new HDRProfileSettings();
                SaveSettings();
                Logs.Add("Created new settings file", false);
            }

            Settings.ApplicationItems.CollectionChanged += ApplicationItems_CollectionChanged;
            _settings.PropertyChanged += Settings_PropertyChanged;
            Logs.LoggingEnabled = Settings.Logging;
            foreach (var application in Settings.ApplicationItems)
            {
                ProcessWatcher.AddProcess(application);
                application.PropertyChanged += ApplicationItem_PropertyChanged;
            }
            Logs.Add("Settings loaded", false);
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
        }

        #endregion Initialization

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (_accessLock)
            {
                try
                {
                    Tools.SetAutoStart(Locale_Texts.HDRProfile, System.Reflection.Assembly.GetEntryAssembly().Location, _settings.AutoStart);

                }
                catch (Exception ex)
                {
                    Logs.AddException(ex);
                }
                if (e.PropertyName.Equals(nameof(Settings.HDRMode)))
                    UpdateHDRBasedOnCurrentApplication();
                Logs.LoggingEnabled = Settings.Logging;
                SaveSettings();
            }
        }



        private void TrayMenu_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Logs.Add("Open app from Tray", false);
            ShowView = true;

        }


        private void StartApplication(ApplicationItem application)
        {
            Logs.Add($"Start application {application.ApplicationName}", false);
            try
            {
                MonitorManager.ActivateHDR();
                System.Threading.Thread.Sleep(3000);
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(application.ApplicationFilePath);
                process.Start();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                System.Threading.Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                Logs.AddException(ex);
                throw ex;
            }
        }

        private void Closing()
        {
            if (Settings.CloseToTray)
            {
                Logs.Add($"Minimizing to tray...", false);
              //  TrayMenuHelper.SwitchTrayIcon(true);
            }
            else
            {
                TrayMenuHelper.SwitchTrayIcon(false);

                Logs.Add($"Shutting down...", false);
                Shutdown();
            }
        }

        private void Shutdown()
        {
            Logs.Add($"Stopping process watcher...", false);
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
                Logs.Add($"Starting HDR Monitoring...", false);
                MonitorManager.StartMonitoring();
                Logs.Add($"HDR Monitoring started", false);
                Started = true;
                UpdateHDRBasedOnCurrentApplication();

            }
        }

        public void Stop()
        {
            lock (_accessLock)
            {
                if (!Started)
                    return;
                Logs.Add($"Stopping HDR Monitoring...", false);
                MonitorManager.StopMonitoring();
                Logs.Add($"HDR Monitoring stopped", false);
                Started = false;
                Logs.Add($"Process watcher stopped", false);

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
                            Logs.Add($"Application added: {((ApplicationItem)applicationItem).ApplicationName}", false);
                            ProcessWatcher.AddProcess(((ApplicationItem)applicationItem));
                            ((ApplicationItem)applicationItem).PropertyChanged += ApplicationItem_PropertyChanged;
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var applicationItem in e.OldItems)
                        {
                            Logs.Add($"Application removed: {((ApplicationItem)applicationItem).ApplicationName}", false);
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
            Logs.Add("Saving settings..", false);
            try
            {
                Settings.SaveSettings(SettingsPath);
                Logs.Add("Settings saved", false);

            }
            catch (Exception ex)
            {
                Logs.AddException(ex);
            }
        }

        private void ApplicationItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSettings();
        }

        private void AddAplication()
        {
            ApplicationAdder adder = new ApplicationAdder();
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
            UpdateHDRBasedOnCurrentApplication();

        }

        private void UpdateHDRBasedOnCurrentApplication()
        {
            lock (_accessLock)
            {
                try
                {
                    bool activateHDR = false;
                    switch (Settings.HDRMode)
                    {
                        case HDRActivationMode.Running:
                            activateHDR = ProcessWatcher.OneProcessIsRunning;
                            break;
                        case HDRActivationMode.Focused:
                            activateHDR = ProcessWatcher.OneProcessIsFocused;
                            break;
                        default:
                            return;

                    }

                    if (activateHDR == HDRIsActive)
                        return;
                    if (activateHDR)
                    {
                        Logs.Add($"Activating HDR...", false);
                        MonitorManager.ActivateHDR();

                    }
                    else if (MonitorManager.GlobalHDRIsActive && Settings.HDRMode != HDRActivationMode.None)
                    {
                        Logs.Add($"Deactivating HDR...", false);
                        if (Settings.GlobalAutoHDR)
                            MonitorManager.DeactivateHDR();
                        else
                        {
                            foreach (Monitor monitor in Settings.Monitors)
                                if (monitor.AutoHDR)
                                    MonitorManager.DeactivateHDR(monitor);
                        }
                    }
                    UpdateRestartAppStates((IDictionary<ApplicationItem, ApplicationState>)ProcessWatcher.Applications, activateHDR);

                    if (MonitorManager.GlobalHDRIsActive)
                        Logs.Add($"HDR is active", false);
                    else
                        Logs.Add($"HDR is inactive", false);
                }
                catch (Exception ex)
                {
                    Logs.AddException(ex);
                    throw ex;
                }
            }
        }

        private void UpdateRestartAppStates(IDictionary<ApplicationItem, ApplicationState> applicationStates, bool restartIfNecessary)
        {
            Dictionary<ApplicationItem, ApplicationState> newLastStates = new Dictionary<ApplicationItem, ApplicationState>();
            foreach (var applicationState in applicationStates)
            {
                if (!applicationState.Key.RestartProcess)
                    continue;
                newLastStates.Add(applicationState.Key, applicationState.Value);
                if (restartIfNecessary)
                {
                    if (!_lastRestartAppStates.ContainsKey(applicationState.Key) && applicationState.Value != ApplicationState.None)
                        RestartProcess(applicationState.Key);
                    else if (_lastRestartAppStates.ContainsKey(applicationState.Key) && applicationState.Value != ApplicationState.None && _lastRestartAppStates[applicationState.Key] == ApplicationState.None)
                        RestartProcess(applicationState.Key);
                }
            }
            _lastRestartAppStates.Clear();
            _lastRestartAppStates = newLastStates;
        }

        private void RestartProcess(ApplicationItem application)
        {
            Logs.Add($"Restarting application {application.ApplicationName}", false);
            foreach (Process process in Process.GetProcessesByName(application.ApplicationName).ToList())
                if (process.StartTime < Process.GetCurrentProcess().StartTime)
                {
                    Logs.Add($"Won't restart application {application.ApplicationName} as it was running before {Locale_Texts.HDRProfile}.", false);

                    return;
                }
            Process.GetProcessesByName(application.ApplicationName).ToList().ForEach(p => p.Kill());
            System.Threading.Thread.Sleep(2000);
            Process proc = new Process();
            StartApplication(application);
        }

        #endregion Process handling

    }

}
