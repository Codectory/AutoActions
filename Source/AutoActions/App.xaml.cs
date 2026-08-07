using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AutoActions.ProjectResources;
using AutoActions.Theming;

namespace AutoActions
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    ///
    public partial class App : Application
    {
        public static Theme Theme { get; set; } = Theme.Light;

        static Mutex mutex;

        [STAThread]
        public static void Main()
         {
         bool createNew = false;
            mutex = new Mutex(true, "{2846416C-610B-4A6B-A31C-A4AA6826E9BE}", out createNew);
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                var application = new App();
                // Resolve the theme before InitializeComponent() so App.xaml merges the matching palette.
                Globals.Instance.LoadSettings();
                Theme = ResolveTheme(Globals.Instance.Settings.ThemeSetting);
                application.InitializeComponent();
                application.Run();
            }
            else
            {
                MessageBox.Show(ProjectLocales.AlreadyRunning);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Dark theme: darken the native title bar of every window (main, dialogs, splash) as it loads.
            if (App.Theme == Theme.Dark)
                EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(OnAnyWindowLoaded));
            Views.AutoActionsMainView mainView = new Views.AutoActionsMainView();
            if (!Globals.Instance.Settings.StartMinimizedToTray)
                mainView.Show();
        }

        private static void OnAnyWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
                WinAPIFunctions.SetWindowDarkMode(window, true);
        }

        private static Theme ResolveTheme(ThemeSetting setting)
        {
            switch (setting)
            {
                case ThemeSetting.Light:
                    return Theme.Light;
                case ThemeSetting.Dark:
                    return Theme.Dark;
                default:
                    return AutoActions.Windows.UI.GetWindowsTheme() == AutoActions.Windows.WindowsTheme.Dark
                        ? Theme.Dark : Theme.Light;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
                mutex.ReleaseMutex();

        }
    }
}
