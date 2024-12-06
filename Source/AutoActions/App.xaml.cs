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
        public static void Main(params string[] parameters)
         {
         bool createNew = false;
            if (parameters.Length == 0)
            {
                mutex = new Mutex(true, "{2846416C-610B-4A6B-A31C-A4AA6826E9BE}", out createNew);
                if (mutex.WaitOne(TimeSpan.Zero, true))
                {
                    var application = new App();
                    application.InitializeComponent();
                    ProjectData.Instance.LoadSettings();
                    application.Run();
                }
                else
                {
                    MessageBox.Show(ProjectLocales.AlreadyRunning);
                }
            }
            else
            {
                ProjectData.Instance.LoadSettings();
                CommandLineHandler.HandleArguments(parameters);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Views.AutoActionsMainView mainView = new Views.AutoActionsMainView();
            if (!ProjectData.Instance.Settings.StartMinimizedToTray)
                mainView.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
                mutex.ReleaseMutex();

        }
    }
}
