using AutoHDR.Profiles.Actions;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
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

        private IProfileAction _applicationStarted;
        public IProfileAction ApplicationStarted
        {
            get { return _applicationStarted; }
            set { _applicationStarted = value; OnPropertyChanged(); }
        }

        private IProfileAction _applicationClosed;
        public IProfileAction ApplicationClosed
        {
            get { return _applicationClosed; }
            set { _applicationClosed = value; OnPropertyChanged(); }
        }

        private IProfileAction _applicationGotFocus;

        public IProfileAction ApplicationGotFocus
        {
            get { return _applicationGotFocus; }
            set { _applicationGotFocus = value; OnPropertyChanged(); }
        }

        private IProfileAction _applicationLostFocus;

        public IProfileAction ApplicationLostFocus
        {
            get { return _applicationLostFocus; }
            set { _applicationLostFocus = value; OnPropertyChanged(); }
        }


    }
}
