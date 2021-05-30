using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Actions
{
    public abstract class ProfileActionGroup
    {
        public string LocalizedGroupName { get; private set; }

        public List<IProfileAction> ProfileActions = new List<IProfileAction>();

        protected ProfileActionGroup(string localizedGroupName)
        {
            LocalizedGroupName = localizedGroupName;
        }
    }
}
