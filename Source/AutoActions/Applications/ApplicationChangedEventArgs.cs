using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions
{
    public enum ApplicationChangedType
    {
        None,
        Started,
        Closed,
        GotFocus,
        LostFocus

    }
    public class ApplicationChangedEventArgs : EventArgs
    {
        public ApplicationItemBase Application { get; }
        public ApplicationChangedType ChangedType { get; }

        public ApplicationChangedEventArgs(ApplicationItemBase application, ApplicationChangedType changedType)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            ChangedType = changedType;
        }
    }
}
