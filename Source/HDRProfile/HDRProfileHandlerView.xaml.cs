using CodectoryCore.UI.Wpf;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HDRProfile
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class HDRProfileHandlerView : MainWindowBase
    {
        public HDRProfileHandlerView()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Properties.Settings.Default.Width = Width;
            Properties.Settings.Default.Height = Height;
            Properties.Settings.Default.Save();
            this.Hide();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

                // for .NET Core you need to add UseShellExecute = true
                // see https://docs.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Width = Properties.Settings.Default.Width;
                Height = Properties.Settings.Default.Height;

            }
            catch  { }       
        }
    }
}
