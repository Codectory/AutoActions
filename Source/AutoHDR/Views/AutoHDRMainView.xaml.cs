using AutoHDR.Properties;
using CodectoryCore.UI.Wpf;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoHDR.Views
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class AutoHDRMainView : MainWindowBase
    {
        readonly object _listResizeLock = new object();
        public AutoHDRMainView()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Size size = new Size(Width, Height);
            Globals.Instance.Settings.WindowSize = size;
            this.Hide();
        }

 


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Globals.Instance.SettingsLoaded += Instance_SettingsLoaded;
                if (Globals.Instance.SettingsLoadedOnce)
                {
                    Width = Globals.Instance.Settings.WindowSize.Width;
                    Height = Globals.Instance.Settings.WindowSize.Height;
                }
            }
            catch  { }       
        }

        private void Instance_SettingsLoaded(object sender, EventArgs e)
        {
            Width = Globals.Instance.Settings.WindowSize.Width;
            Height = Globals.Instance.Settings.WindowSize.Height;
        }
    }
}
