using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
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
        private MenuItem _activateHDRButton;
        private MenuItem _deactivateHDRButton;

        public event EventHandler OpenViewRequested;
        public event EventHandler CloseApplicationRequested;


        public event EventHandler<string> NewLog;


        public void Initialize()
        {
            if (Initialized)
                return;
            CallNewLog("Initializing tray menu");
            try
            {
                _trayMenu = new TaskbarIcon();
                _trayMenu.Visibility = Visibility.Hidden;
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
                _activateHDRButton = new MenuItem()
                {
                    Header = Locale_Texts.ActivateHDR
                };

                _deactivateHDRButton = new MenuItem()
                {
                    Header = Locale_Texts.DeactivateHDR
                };
                _openButton.Click += (o, e) => OpenViewRequested?.Invoke(this, EventArgs.Empty);
                _closeButton.Click += (o, e) => CloseApplicationRequested?.Invoke(this, EventArgs.Empty);
                _activateHDRButton.Click += (o, e) => HDRController.ActivateHDR();
                _deactivateHDRButton.Click += (o, e) => HDRController.DeactivateHDR();
                contextMenu.Items.Add(_openButton);
                contextMenu.Items.Add(_activateHDRButton);
                contextMenu.Items.Add(_deactivateHDRButton);
                contextMenu.Items.Add(_closeButton);

                HDRController.HDRIsActiveChanged += HDRController_HDRIsActiveChanged;
                _activateHDRButton.IsEnabled = !HDRController.HDRIsActive;
                _deactivateHDRButton.IsEnabled = HDRController.HDRIsActive;
                _trayMenu.ContextMenu = contextMenu;
                _trayMenu.TrayLeftMouseDown += TrayMenu_TrayLeftMouseDown;
                CallNewLog("Tray menu initialized");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void HDRController_HDRIsActiveChanged(object sender, EventArgs e)
        {
            _activateHDRButton.IsEnabled = !HDRController.HDRIsActive;
            _deactivateHDRButton.IsEnabled = HDRController.HDRIsActive;
        }

        private void TrayMenu_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            SwitchTrayIcon(false);
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
