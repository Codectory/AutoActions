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
        public abstract string LocalizedName { get; }

        public abstract ActionEndResult RunAction();
    }
}
