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
        private MenuItem _actions;


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
                _openButton.Click += (o, e) => OpenViewRequested?.Invoke(this, EventArgs.Empty);
                _closeButton.Click += (o, e) => CloseApplicationRequested?.Invoke(this, EventArgs.Empty);
                contextMenu.Items.Add(_openButton);
                contextMenu.Items.Add(_closeButton);
                InitializeActionsMenuItem(contextMenu);
                _trayMenu.ContextMenu = contextMenu;
                _trayMenu.TrayLeftMouseDown += TrayMenu_TrayLeftMouseDown;
                CallNewLog("Tray menu initialized");
                SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        readonly object _lockActions = new object();

        private void InitializeActionsMenuItem(ContextMenu contextMenu)
        {
            _actions = new MenuItem()
            {
                Header = ProjectLocales.Actions
            };
            contextMenu.Items.Add(_actions);
            Globals.Instance.Settings.ActionShortcuts.CollectionChanged += (o, e) =>
                {
                    UpdateActionItems();
                };
            UpdateActionItems();
        }

        private void UpdateActionItems()
        {
            lock (_lockActions)
            {
                _actions.Items.Clear();
                foreach (var action in Globals.Instance.Settings.ActionShortcuts)
                {
                    MenuItem item = new MenuItem();
                    item.Header = $"[{action.ActionTypeName}]{action.ActionDescription}";
                    item.Click += (o, e) => action.RunAction(ApplicationChangedType.None);
                    _actions.Items.Add(item);
                }
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
