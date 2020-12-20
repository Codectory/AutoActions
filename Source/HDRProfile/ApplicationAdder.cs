using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace HDRProfile
{
    public class ApplicationAdder : BaseViewModel
    {
        private bool _canCreate = false;
        private string _displayName = string.Empty;
        private string _filePath = string.Empty;
        private ApplicationItem applicationItem = null;

        public ApplicationItem ApplicationItem { get => applicationItem; private set { applicationItem = value; OnPropertyChanged(); } }

        public RelayCommand GetFileCommand { get; private set; }
        public RelayCommand  CreateApplicationItemCommand { get; private set; }


        public ApplicationAdder()
        {
            CreateRelayCommands();
        }

        private void CreateRelayCommands()
        {
            GetFileCommand = new RelayCommand(GetFile);
            CreateApplicationItemCommand = new RelayCommand(CreateApplicationItem);
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
        }

        public void CreateApplicationItem()
        {

            ApplicationItem = new ApplicationItem(DisplayName, FilePath);
        }
    }
}
