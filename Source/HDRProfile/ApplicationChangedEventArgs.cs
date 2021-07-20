using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR
{
    public enum ApplicationChangedType
    {
        Started,
        Closed,
        GotFocus,
        LostFocus
    }
    public class ApplicationChangedEventArgs : EventArgs
    {
        public ApplicationItem Application { get; }
        public ApplicationChangedType ChangedType { get; }

        public ApplicationChangedEventArgs(ApplicationItem application, ApplicationChangedType changedType)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            ChangedType = changedType;
        }
    }
}
