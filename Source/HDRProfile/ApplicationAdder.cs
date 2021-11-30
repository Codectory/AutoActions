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
using AutoHDR.UWP;
using AutoHDR.ProjectResources;

namespace AutoHDR
{
    public class ApplicationAdder : DialogViewModelBase
    {
        private bool _canCreate = false;
        private string _displayName = string.Empty;
        private string _filePath = string.Empty;
        private ApplicationItem applicationItem = null;

        public ApplicationItem ApplicationItem { get => applicationItem; private set { applicationItem = value; OnPropertyChanged(); } }

        public RelayCommand GetFileCommand { get; private set; }
        public RelayCommand GetUWPAppCommand { get; private set; }

        public RelayCommand<object>  OKClickCommand { get; private set; }

        public event EventHandler OKClicked;

        public ApplicationAdder()
        {
            Title = Locale_Texts.AddApplication;
            CreateRelayCommands();
        }

        private void CreateRelayCommands()
        {
            GetFileCommand = new RelayCommand(GetFile);
            GetUWPAppCommand = new RelayCommand(GetUWPAplication);
            OKClickCommand = new RelayCommand<object>(CreateApplicationItem);
        }


        public string DisplayName { get => _displayName; set { _displayName = value; UpdateCanCreate(); OnPropertyChanged(); } }

        public string FilePath { get => _filePath; set { _filePath = value; UpdateCanCreate(); OnPropertyChanged(); } }

        private void UpdateCanCreate()
        {
            CanCreate = !String.IsNullOrEmpty(FilePath) && !String.IsNullOrEmpty(DisplayName);
        }

        public bool CanCreate { get => _canCreate; set { _canCreate = value; OnPropertyChanged(); } }

        public void GetFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".exe";
            fileDialog.Filter = "Executables (.exe)|*.exe";
            Nullable<bool> result = fileDialog.ShowDialog();
            string filePath = string.Empty;
            if (result == true)
                filePath = fileDialog.FileName;
            else
                return;
            if (!File.Exists(filePath))
                throw new Exception("Invalid file path.");
            FilePath = filePath;
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = new FileInfo(FilePath).Name.Replace(".exe", "");
            ApplicationItem = new ApplicationItem(DisplayName, FilePath);

        }

        public void CreateApplicationItem(object parameter)
        {
            if (applicationItem != null)
                ApplicationItem.DisplayName = DisplayName;
            OKClicked?.Invoke(this, EventArgs.Empty);
            CloseDialog(parameter as Window);
        }

        private void GetUWPAplication()
        {
            UIServices.SetBusyState();
            UWPApplicationDialog uwpDialog = new UWPApplicationDialog();
            uwpDialog.OKClicked += (o, e) =>
            {
                if (uwpDialog.ApplicationItem != null)
                {
                    ApplicationItem = uwpDialog.ApplicationItem;
                    DisplayName = ApplicationItem.DisplayName;
                    FilePath = ApplicationItem.ApplicationFilePath;
                }
            };
            if (DialogService != null)
                DialogService.ShowDialogModal(uwpDialog, new System.Drawing.Size(800,600));
        }

    }
}
