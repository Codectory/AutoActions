using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HDRProfile
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


        public void Initialize(MonitorManager monitorManager)
        {
            if (Initialized)
                return;
            CallNewLog("Initializing tray menu");
            try
            {
                _trayMenu = new TaskbarIcon();
                _trayMenu.Visibility = Visibility.Visible;
                _trayMenu.ToolTipText = Locale_Texts.HDRProfile;
                _trayMenu.Icon = Locale_Texts.Logo;
                ContextMenu contextMenu = new ContextMenu();
                _openButton = new MenuItem()
                {
                    Header = Locale_Texts.Open
                };

                _closeButton = new MenuItem()
                {
                    Header = Locale_Texts.Shutdown
                };
                _hdrSwitchButton = new MenuItem()
                {
                    Header = Locale_Texts.ActivateHDR
                };
                _openButton.Click += (o, e) => OpenViewRequested?.Invoke(this, EventArgs.Empty);
                _closeButton.Click += (o, e) => CloseApplicationRequested?.Invoke(this, EventArgs.Empty);
                _hdrSwitchButton.Click += (o, e) => { if (MonitorManager.GlobalHDRIsActive) monitorManager.DeactivateHDR(); else monitorManager.ActivateHDR(); };
                contextMenu.Items.Add(_openButton);
                contextMenu.Items.Add(_hdrSwitchButton);
                contextMenu.Items.Add(_closeButton);
                _trayMenu.ContextMenu = contextMenu;
                _trayMenu.TrayLeftMouseDown += TrayMenu_TrayLeftMouseDown;
                MonitorManager.HDRIsActiveChanged += HDRController_HDRIsActiveChanged;
                UpdateMenuButtons();
                CallNewLog("Tray menu initialized");

            }
            catch (Exception ex)
            {
                throw ex;
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
                _hdrSwitchButton.Header = MonitorManager.GlobalHDRIsActive ? Locale_Texts.DeactivateHDR : Locale_Texts.ActivateHDR;
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
