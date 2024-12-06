using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Applications
{
    public interface IApplicationProvider
    {
        string ProviderName { get; }
        string LocalizedCaption { get; }

        List<ApplicationItemBase> GetApplications();
    }
}
