using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows;
using System.Collections.ObjectModel;

namespace HDRProfile.UWP
{
    public class UWPApplicationDialog : DialogViewModelBase
    {
        private bool _canCreate = false;
        private ObservableCollection<ApplicationItem> _uwpApplications = new ObservableCollection<ApplicationItem>();
        public ObservableCollection<ApplicationItem> UWPApplications { get => _uwpApplications;  set { _uwpApplications = value; OnPropertyChanged(); } }


        private ApplicationItem applicationItem = null;

        public ApplicationItem ApplicationItem { get => applicationItem;  set { applicationItem = value; OnPropertyChanged(); } }




        public RelayCommand<object>  OKClickCommand { get; private set; }

        public event EventHandler OKClicked;

        public UWPApplicationDialog()
        {
            Title = Locale_Texts.ChooseUWPApplication;
            foreach (ApplicationItem app in UWP.UWPAppsManager.GetUWPApps())
                UWPApplications.Add(app);
            CreateRelayCommands();
        }

        private void CreateRelayCommands()
        {
            OKClickCommand = new RelayCommand<object>(CreateApplicationItem);
        }

        private void UpdateCanCreate()
        {
            CanCreate = ApplicationItem !=null;
        }

        public bool CanCreate { get => _canCreate; set { _canCreate = value; OnPropertyChanged(); } }


        public void CreateApplicationItem(object parameter)
        {
            OKClicked?.Invoke(this, EventArgs.Empty);
            CloseDialog(parameter as Window);
        }
    }
}
