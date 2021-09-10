using AutoHDR.Profiles.Actions;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Profiles
{
    public class Profile : BaseViewModel
    {
        private ApplicationItem _application;

        public ApplicationItem Application
        {
            get { return _application; }
            set { _application = value;  OnPropertyChanged(); }
        }

        private ProfileMode _mode = ProfileMode.OnRunning;

        public ProfileMode Mode
        {
            get { return _mode; }
            set { _mode = value;  OnPropertyChanged(); }
        }

        private bool _restartOnOccurance = false;

        public bool RestartOnOccurance
        {
            get { return _restartOnOccurance; }
            set { _restartOnOccurance = value; OnPropertyChanged(); }
        }

        private ObservableCollection<IProfileAction> _applicationStarted;
        public ObservableCollection<IProfileAction> ApplicationStarted
        {
            get { return _applicationStarted; }
            set { _applicationStarted = value;  OnPropertyChanged(); }
        }

        private ObservableCollection<IProfileAction> _applicationClosed;
        public ObservableCollection<IProfileAction> ApplicationClosed
        {
            get { return _applicationClosed; }
            set { _applicationClosed = value; OnPropertyChanged(); }
        }

        private ObservableCollection<IProfileAction> _applicationGotFocus;

        public ObservableCollection<IProfileAction> ApplicationGotFocus
        {
            get { return _applicationGotFocus; }
            set { _applicationGotFocus = value; OnPropertyChanged(); }
        }

        private ObservableCollection<IProfileAction> _applicationLostFocus;

        public ObservableCollection<IProfileAction> ApplicationLostFocus
        {
            get { return _applicationLostFocus; }
            set { _applicationLostFocus = value; OnPropertyChanged(); }
        }


    }
}
