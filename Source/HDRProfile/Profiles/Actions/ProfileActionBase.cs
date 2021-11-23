using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AutoHDR.Profiles.Actions
{
    public abstract class ProfileActionBase : BaseViewModel, IProfileAction
    {
        public abstract string ActionDescription { get; }
        public abstract string ActionTypeName { get; }
        public abstract ActionEndResult RunAction(params object[] parameter);

    }
}
