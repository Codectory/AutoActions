using AutoActions.Properties;
using CodectoryCore.UI.Wpf;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AutoActions.Views
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class AutoActionsMainView : MainWindowBase
    {
        readonly object _listResizeLock = new object();
        public AutoActionsMainView()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Size size = new Size(Width, Height);
            ProjectData.Instance.Settings.WindowSize = size;
            this.Hide();
        }

 


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ProjectData.Instance.SettingsLoaded += Instance_SettingsLoaded;
                if (ProjectData.Instance.SettingsLoadedOnce)
                {
                    Width = ProjectData.Instance.Settings.WindowSize.Width;
                    Height = ProjectData.Instance.Settings.WindowSize.Height;
                }
            }
            catch  { }       
        }

        private void Instance_SettingsLoaded(object sender, EventArgs e)
        {
            Width = ProjectData.Instance.Settings.WindowSize.Width;
            Height = ProjectData.Instance.Settings.WindowSize.Height;
        }
    }
}
