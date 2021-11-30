using AutoHDR.Properties;
using CodectoryCore.UI.Wpf;
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
            Globals.Instance.Settings.ApplicationProfileAssignments.CollectionChanged += ApplicationProfileAssignments_CollectionChanged;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Globals.Instance.Settings.ApplicationProfileAssignments.CollectionChanged -= ApplicationProfileAssignments_CollectionChanged;
            Properties.Settings.Default.Width = Width;
            Properties.Settings.Default.Height = Height;
            Properties.Settings.Default.Save();

            this.Hide();
        }

        private void ApplicationProfileAssignments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ResizeApplicationList();
        }

        private void ResizeApplicationList()
        {
            lock (_listResizeLock)
            {

                GridView gv = ApplicationList.View as GridView;
                foreach (var column in gv.Columns)
                {
                    column.Width = column.ActualWidth;
                    column.Width = double.NaN;
                }
            }
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
