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
        private string SettingsPath => $"{System.AppDomain.CurrentDomain.BaseDirectory}\\HDRProfile_Settings.xml";
        TaskbarIcon TrayMenu;
        readonly object _accessLock = new object();

        private bool started = false;
        public bool Started { get => started; private set { started = value; OnPropertyChanged(); } }
        ProcessWatcher ProcessWatcher;
        HDRController HDRSwitcherHandler;
        private bool _showView = false;
        private HDRProfileSettings settings;

        public bool ShowView { get => _showView;  set { _showView = value; OnPropertyChanged(); } }

        public RelayCommand ActivateHDRCommand { get; private set; }
        public RelayCommand DeactivateHDRCommand { get; private set; }
        public RelayCommand AddApplicationCommand { get; private set; }
        public RelayCommand<ApplicationItem> RemoveApplicationCommand { get; private set; }
        public RelayCommand LoadingCommand { get; private set; }
        public RelayCommand ClosingCommand { get; private set; }
        public RelayCommand ShutdownCommand { get; private set; }


        public HDRProfileSettings Settings { get => settings; set { settings = value; OnPropertyChanged(); } }

        public bool Initialized { get; private set; } = false;

        public HDRProfileHandler()
        {
           // ChangeLanguage( new System.Globalization.CultureInfo("en-US"));
            Initialize();
        }

        private void ChangeLanguage(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture.Name);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(culture.Name);
        }

        private void Initialize()
        {

            lock (_accessLock)
            {
                if (Initialized)
                    return;

                ProcessWatcher = new ProcessWatcher();
                HDRSwitcherHandler = new HDRController();
                LoadSettings();
                InitializeTrayMenu();
                CreateRelayCommands();
                SwitchTrayIcon(Settings.StartMinimizedToTray);
                ShowView = !Settings.StartMinimizedToTray;
                Initialized = true;
            }
        }

        private void LoadSettings()
        {
            try
            {
                Settings = HDRProfileSettings.ReadSettings(SettingsPath);
            }
            catch (Exception)
            {
                Settings = new HDRProfileSettings();
                Settings.SaveSettings(SettingsPath);
            }
            Settings.ApplicationItems.CollectionChanged += ApplicationItems_CollectionChanged;
            settings.PropertyChanged += Settings_PropertyChanged;
            foreach (var proc in Settings.ApplicationItems)
            {
                ProcessWatcher.AddProcess(proc.ApplicationName.ToUpperInvariant());
            }
            SetProcessWatchMode();

        }

        private void SetProcessWatchMode()
        {
            switch (settings.HDRMode)
            {
                case HDRMode.None:
                    ProcessWatcher.WatchMode = ProcessWatchMode.None;
                    break;
                case HDRMode.Focused:
                    ProcessWatcher.WatchMode = ProcessWatchMode.Focused;
                    break;
                case HDRMode.Running:
                    ProcessWatcher.WatchMode = ProcessWatchMode.Running;
                    break;
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (_accessLock)
            {

                if (e.PropertyName == nameof(Settings.HDRMode))
                {
                    SetProcessWatchMode();
                }
                SetAutoStart(Locale_Texts.HDRProfile, System.Reflection.Assembly.GetEntryAssembly().Location, settings.AutoStart);
                Settings.SaveSettings(SettingsPath);
            }
        }

        public static void SetAutoStart(string applicationName, string filePath, bool autostart)
        {
            RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            object existing = rk.GetValue(applicationName);
            if (filePath.Equals(existing) && autostart)
                return;

            if (autostart)
                rk.SetValue(applicationName, filePath);
            else
                rk.DeleteValue(applicationName, false);
        }


        private void InitializeTrayMenu()
        {
            TrayMenu = new TaskbarIcon();
            TrayMenu.Visibility = Visibility.Hidden;
            TrayMenu.ToolTipText = Locale_Texts.HDRProfile;
            TrayMenu.Icon = Locale_Texts.Logo;
            ContextMenu contextMenu = new ContextMenu();
            MenuItem close = new MenuItem()
            {
                Header = Locale_Texts.Shutdown
            };
            close.Click += (o, e) => Shutdown();

            MenuItem open = new MenuItem()
            {
                Header = Locale_Texts.Open
            };
            open.Click += (o, e) => SwitchTrayIcon(false);

            contextMenu.Items.Add(open);
            contextMenu.Items.Add(close);
            TrayMenu.ContextMenu = contextMenu;
            TrayMenu.TrayLeftMouseDown += TrayMenu_TrayLeftMouseDown;
        }

        private void TrayMenu_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            SwitchTrayIcon(false);
            ShowView = true;

        }

        private void CreateRelayCommands()
        {
            ActivateHDRCommand = new RelayCommand(HDRSwitcherHandler.ActivateHDR);
            DeactivateHDRCommand = new RelayCommand(HDRSwitcherHandler.DeactivateHDR);
            AddApplicationCommand = new RelayCommand(AddAplication);
            RemoveApplicationCommand = new RelayCommand<ApplicationItem>(RemoveApplication);
            LoadingCommand = new RelayCommand(Starting);
            ClosingCommand = new RelayCommand(Closing);
            ShutdownCommand = new RelayCommand(Shutdown);
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
                            ProcessWatcher.AddProcess(((ApplicationItem)applicationItem).ApplicationName.ToUpperInvariant());
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var applicationItem in e.OldItems)
                            ProcessWatcher.RemoveProcess(((ApplicationItem)applicationItem).ApplicationName.ToUpperInvariant());
                        break;

                }
                Settings.SaveSettings(SettingsPath);
            }
        }
        private void AddAplication()
        {
            ApplicationAdder adder = new ApplicationAdder();
            ApplicationAdderView adderView = new ApplicationAdderView();
            adderView.DataContext = adder;
            adderView.ShowDialog();
            if (adder.ApplicationItem == null)
                return;

            if (!Settings.ApplicationItems.Any(pi => pi.ApplicationFilePath == adder.ApplicationItem.ApplicationFilePath))
                Settings.ApplicationItems.Add(adder.ApplicationItem);

        }

        private void RemoveApplication(ApplicationItem process)
        {
            Settings.ApplicationItems.Remove(process);

        }
        #endregion Process handling

        private void Starting()
        {
            Start();
        }
        private void Closing()
        {
            if (Settings.CloseToTray)
            {
                SwitchTrayIcon(true);
            }
            else
            {
                Shutdown();
            }
        }

        private void Shutdown()
        {
            Stop();
            SwitchTrayIcon(false);
            Application.Current.Shutdown();
        }

        private void SwitchTrayIcon(bool showTray)
        {
            TrayMenu.Visibility = showTray ? System.Windows.Visibility.Visible : Visibility.Hidden;
        }

        private void ProcessWatcher_ChangeHDRMode(object sender, EventArgs e)
        {
            lock (_accessLock)
            {
                if ((Settings.HDRMode == HDRMode.Running && ProcessWatcher.OneProcessIsRunning) ||Settings.HDRMode == HDRMode.Focused && ProcessWatcher.OneProcessIsFocused)
                    HDRSwitcherHandler.ActivateHDR();
                else
                    HDRSwitcherHandler.DeactivateHDR();
            }

        }

        public void Start()
        {
            if (Started)
                return;
            lock (_accessLock)
            {
                ProcessWatcher.OneProcessIsRunningChanged += ProcessWatcher_ChangeHDRMode;
                ProcessWatcher.OneProcessIsFocusedhanged += ProcessWatcher_ChangeHDRMode;

                Started = true;
                ProcessWatcher.Start();
            }
        }

        public void Stop()
        {
            if (!Started)
                return;
            lock (_accessLock)
            {

                ProcessWatcher.OneProcessIsRunningChanged -= ProcessWatcher_ChangeHDRMode;
                ProcessWatcher.OneProcessIsFocusedhanged -= ProcessWatcher_ChangeHDRMode;

                ProcessWatcher.Stop();
                HDRSwitcherHandler.DeactivateHDR();
                Started = false;
            }

        }
    }

}
