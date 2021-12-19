using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AutoHDR.ProjectResources;
using AutoHDR.Displays;
using Microsoft.Win32;

namespace AutoHDR
{
    public class TrayMenuHelper
    {
        public bool Initialized { get; private set; }
        TaskbarIcon _trayMenu;

        private MenuItem _openButton;
        private MenuItem _closeButton;
        private MenuItem _hdrSwitchButton;

        public event EventHandler OpenViewRequested;
        public event EventHandler CloseApplicationRequested;


        public event EventHandler<string> NewLog;


        public void Initialize(IDisplayManagerBase displayManager)
        {
            if (Initialized)
                return;
            CallNewLog("Initializing tray menu");
            try
            {
                _trayMenu = new TaskbarIcon();
                _trayMenu.Visibility = Visibility.Visible;
                _trayMenu.ToolTipText = ProjectLocales.AutoHDR;
                _trayMenu.Icon = ProjectLocales.MainIcon;
                ContextMenu contextMenu = new ContextMenu();
                _openButton = new MenuItem()
                {
                    Header = ProjectLocales.Open
                };

                _closeButton = new MenuItem()
                {
                    Header = ProjectLocales.Shutdown
                };
                _hdrSwitchButton = new MenuItem()
                {
                    Header = ProjectLocales.ActivateHDR
                };
                _openButton.Click += (o, e) => OpenViewRequested?.Invoke(this, EventArgs.Empty);
                _closeButton.Click += (o, e) => CloseApplicationRequested?.Invoke(this, EventArgs.Empty);
                _hdrSwitchButton.Click += (o, e) => { if (DisplayManagerHandler.Instance.GlobalHDRIsActive) displayManager.DeactivateHDR(); else displayManager.ActivateHDR(); };
                contextMenu.Items.Add(_openButton);
                contextMenu.Items.Add(_hdrSwitchButton);
                contextMenu.Items.Add(_closeButton);
                _trayMenu.ContextMenu = contextMenu;
                _trayMenu.TrayLeftMouseDown += TrayMenu_TrayLeftMouseDown;
                DisplayManagerHandler.Instance.HDRIsActiveChanged += HDRController_HDRIsActiveChanged;
                UpdateMenuButtons();
                CallNewLog("Tray menu initialized");
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                SwitchTrayIcon(false);
            }
            else if (e.Mode == PowerModes.Resume)
            {
                System.Threading.Thread.Sleep(5000);
                SwitchTrayIcon(true);
            }
        }

        private void HDRController_HDRIsActiveChanged(object sender, EventArgs e)
        {
            UpdateMenuButtons();
        }

        private void UpdateMenuButtons()
        {
            Application.Current.Dispatcher.Invoke(
            (Action)(() =>
            {
                _hdrSwitchButton.Header = DisplayManagerHandler.Instance.GlobalHDRIsActive ? ProjectLocales.DeactivateHDR : ProjectLocales.ActivateHDR;
            }));
        }

        private void TrayMenu_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            //SwitchTrayIcon(false);
            OpenViewRequested?.Invoke(this, EventArgs.Empty);

        }

        private void CallNewLog(string message)
        {
            NewLog?.Invoke(this, message);
        }

        public void SwitchTrayIcon(bool showTray)
        {
            _trayMenu.Visibility = showTray ? System.Windows.Visibility.Visible : Visibility.Hidden;
        }

    }
}
