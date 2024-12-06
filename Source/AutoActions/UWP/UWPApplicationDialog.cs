using AutoActions.ProjectResources;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace AutoActions.UWP
{
    public class UWPApplicationDialog : DialogViewModelBase
    {
        private bool _canCreate = false;
        private ObservableCollection<ApplicationItemBase> _uwpApplications = new ObservableCollection<ApplicationItemBase>();
        public ObservableCollection<ApplicationItemBase> UWPApplications { get => _uwpApplications;  set { _uwpApplications = value; OnPropertyChanged(); } }


        private ApplicationItemBase applicationItem = null;

        public ApplicationItemBase ApplicationItem { get => applicationItem;  set { applicationItem = value; OnPropertyChanged(); } }




        public RelayCommand<object>  OKClickCommand { get; private set; }

        public event EventHandler OKClicked;

        public UWPApplicationDialog()
        {
            Title = ProjectLocales.ChooseUWPApplication;
            foreach (ApplicationItemBase app in UWP.UWPAppsManager.Instance.GetApplications())
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
