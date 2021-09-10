using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Profiles.Actions
{
    public abstract class BaseProfileAction : BaseViewModel, IProfileAction
    {
        public abstract string LocalizeCaption { get; }
        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        public abstract ActionEndResult RunAction();
    }
}
