using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HDRProfile
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    ///

    public partial class App : Application
    {
        static Mutex mutex = new Mutex(true, "{4DF0C02E-F86D-4F5F-93A9-DC9F8EDF1AD5}");
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                mutex.ReleaseMutex();

            }
            else
            {
                MessageBox.Show(Locale_Texts.AlreadyRunning);
                Application.Current.Shutdown();
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }
    }
}
