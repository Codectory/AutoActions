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

        private bool hdrIsActive;
        private static Logs Logs = new Logs($"{System.AppDomain.CurrentDomain.BaseDirectory}HDRProfile.log", "HDRPProfile", Assembly.GetExecutingAssembly().GetName().Version.ToString(), true);
        private HDRProfileSettings settings;

        Dictionary<ApplicationItem, ApplicationState> _lastApplicationStates = new Dictionary<ApplicationItem, ApplicationState>();

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

        public HDRProfileSettings Settings { get => settings; set { settings = value; OnPropertyChanged(); } }
        public bool Initialized { get; private set; } = false;
        public bool ShowView { get => _showView; set { _showView = value; OnPropertyChanged(); } }

        public ApplicationItem CurrentApplication { get => _currentApplication; set { _currentApplication = value; OnPropertyChanged(); } }

        public bool HDRIsActive { get => hdrIsActive; set { hdrIsActive = value; OnPropertyChanged(); } }
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
                HDRController.HDRIsActiveChanged += HDRController_HDRIsActiveChanged;
                HDRIsActive = HDRController.HDRIsActive;
                LoadSettings();
                InitializeTrayMenuHelper();
                CreateRelayCommands();
                ShowView = !Settings.StartMinimizedToTray;
                Initialized = true;
                Logs.Add("Initialized", false);
                Start();

            }
        }

        private void LogSystem()
        {
        }

        private void HDRController_HDRIsActiveChanged(object sender, EventArgs e)
        {
            HDRIsActive = HDRController.HDRIsActive;
        }

        private void InitializeTrayMenuHelper()
        {
            TrayMenuHelper = new TrayMenuHelper();
            TrayMenuHelper.Initialize();
            TrayMenuHelper.OpenViewRequested += TrayMenuHelper_OpenViewRequested;
            TrayMenuHelper.CloseApplicationRequested += TrayMenuHelper_CloseApplicationRequested;
            TrayMenuHelper.SwitchTrayIcon(Settings.StartMinimizedToTray);
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
            settings.PropertyChanged += Settings_PropertyChanged;
            Logs.LoggingEnabled = Settings.Logging;
            foreach (var application in Settings.ApplicationItems)
            {
                ProcessWatcher.AddProcess(application, false);
                application.PropertyChanged += ApplicationItem_PropertyChanged;
            }
            Logs.Add("Settings loaded", false);
        }


        private void CreateRelayCommands()
        {
            ActivateHDRCommand = new RelayCommand(HDRController.ActivateHDR);
            DeactivateHDRCommand = new RelayCommand(HDRController.DeactivateHDR);
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
                    Tools.SetAutoStartInScheduler(Locale_Texts.HDRProfile, System.Reflection.Assembly.GetEntryAssembly().Location, settings.AutoStart);

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
                HDRController.ActivateHDR();
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
                TrayMenuHelper.SwitchTrayIcon(true);
            }
            else
            {
                Logs.Add($"Shutting down...", false);
                Shutdown();
            }
        }

        private void Shutdown()
        {
            Stop();
            TrayMenuHelper.SwitchTrayIcon(false);
            Application.Current.Shutdown();
        }

        public void Start()
        {
            lock (_accessLock)
            {
                if (Started)
                    return;
                Logs.Add($"Starting HDR Monitoring...", false);
                HDRController.StartMonitoring();
                Logs.Add($"HDR Monitoring started", false);
                Logs.Add($"Starting process watcher...", false);
                ProcessWatcher.OneProcessIsRunningChanged += ProcessWatcher_RunningOrFocusedChanged;
                ProcessWatcher.OneProcessIsFocusedChanged += ProcessWatcher_RunningOrFocusedChanged;
                Started = true;
                ProcessWatcher.Start();
                UpdateHDRBasedOnCurrentApplication();
                Logs.Add($"Process watcher started", false);

            }
        }

        public void Stop()
        {
            lock (_accessLock)
            {
                if (!Started)
                    return;
                Logs.Add($"Stopping HDR Monitoring...", false);
                HDRController.StopMonitoring();
                Logs.Add($"HDR Monitoring stopped", false);
                Logs.Add($"Stopping process watcher...", false);
                ProcessWatcher.OneProcessIsRunningChanged -= ProcessWatcher_RunningOrFocusedChanged;
                ProcessWatcher.OneProcessIsFocusedChanged -= ProcessWatcher_RunningOrFocusedChanged;
                ProcessWatcher.Stop();
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
            UpdateHDRBasedOnCurrentApplication();

        }

        private void UpdateHDRBasedOnCurrentApplication()
        {
            lock (_accessLock)
            {
                try
                {
                    bool hdrTargetState = false;
                    switch (Settings.HDRMode)
                    {
                        case HDRActivationMode.Running:
                            CurrentApplication = ProcessWatcher.CurrentRunningApplicationItem;
                            hdrTargetState = ProcessWatcher.OneProcessIsRunning;
                            break;
                        case HDRActivationMode.Focused:
                            CurrentApplication = ProcessWatcher.CurrentFocusedApplicationItem;
                            hdrTargetState = ProcessWatcher.OneProcessIsFocused;
                            break;
                        default:
                            CurrentApplication = null;
                            return;

                    }

                    if (hdrTargetState == HDRIsActive)
                        return;
                    if (hdrTargetState)
                    {
                        Logs.Add($"Activating HDR...", false);

                        HDRController.ActivateHDR();
                        CheckIfRestartIsNecessary((IDictionary<ApplicationItem, ApplicationState>)ProcessWatcher.Applications);

                    }
                    else if (HDRController.HDRIsActive && Settings.HDRMode != HDRActivationMode.None)
                    {
                        Logs.Add($"Deactivating HDR...", false);
                        HDRController.DeactivateHDR();
                    }
                    if (HDRController.HDRIsActive)
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

        private void CheckIfRestartIsNecessary(IDictionary<ApplicationItem, ApplicationState> applicationStates)
        {
            Dictionary<ApplicationItem, ApplicationState> newLastStates = new Dictionary<ApplicationItem, ApplicationState>();
            foreach (var applicationState in applicationStates)
            {
                if (!applicationState.Key.RestartProcess)
                    continue;
                newLastStates.Add(applicationState.Key, applicationState.Value);
                if (!_lastApplicationStates.ContainsKey(applicationState.Key) && applicationState.Value != ApplicationState.None)
                    RestartProcess(applicationState.Key);
                else if (_lastApplicationStates.ContainsKey(applicationState.Key) && applicationState.Value != ApplicationState.None && _lastApplicationStates[applicationState.Key] == ApplicationState.None)
                    RestartProcess(applicationState.Key);
            }
            _lastApplicationStates.Clear();
            _lastApplicationStates = newLastStates;
        }

        private void RestartProcess(ApplicationItem application)
        {
            Logs.Add($"Restarting application {application.ApplicationName}", false);
            Process.GetProcessesByName(application.ApplicationName).ToList().ForEach(p => p.Kill());
            Process proc = new Process();
            StartApplication(application);
        }

        #endregion Process handling

    }

}
