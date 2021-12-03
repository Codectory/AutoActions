using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Info
{
    public class LogsStorage : DialogViewModelBase
    {
        SortableObservableCollection<LogEntry> _entries = new SortableObservableCollection<LogEntry>();

        public SortableObservableCollection<LogEntry> Entries

        {
            get => _entries;
            private set { _entries = value; OnPropertyChanged(); }
        }

        readonly object _lockLogs = new object();

        public LogsStorage()
        {
            Globals.Logs.NewLog += Logs_NewLog;
        }

        private void Logs_NewLog(object sender, LogEntry e)
        {
            lock (_lockLogs)
            {
                Entries.Add(e);
                Entries.Sort(x => x.Date, System.ComponentModel.ListSortDirection.Ascending);
            }
        }

    }
}
