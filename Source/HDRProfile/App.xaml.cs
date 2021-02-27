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
        static Mutex mutex = new Mutex(true, "{2846416C-610B-4A6B-A31C-A4AA6826E9BE}");
        protected override void OnStartup(StartupEventArgs e)
        {

            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show(Locale_Texts.AlreadyRunning);
                Application.Current.Shutdown();
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            mutex.ReleaseMutex();

        }
    }
}
