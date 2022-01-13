using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AutoActions.Profiles.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ProfileActionBase : BaseViewModel, IProfileAction
    {
        public EventHandler<LogEntry> NewLog { get; set; }
        public abstract string ActionDescription { get; }
        public abstract string ActionTypeName { get; }
        public abstract bool CanSave { get; }
        public abstract string CannotSaveMessage { get; }
        public abstract ActionEndResult RunAction(ApplicationChangedType applicationChangedType);

        public ProfileActionBase()
        {
            PropertyChanged += ProfileActionBase_PropertyChanged;
        }

        private void ProfileActionBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ActionDescription))
                OnPropertyChanged(nameof(ActionDescription));
        }

        protected void CallNewLog(LogEntry entry)
        {
            if (NewLog != null)
            {
                foreach (EventHandler<LogEntry> handler in NewLog.GetInvocationList())
                    handler.BeginInvoke(this, entry, null, null);
            }
        }

    }
}
