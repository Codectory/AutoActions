using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Profiles.Actions
{
    public class ActionTypeDescription
    {
        public Type ActionType { get; }
        public string TypeDescription { get; }

        public ActionTypeDescription(Type actionType, string typeDescription)
        {
            ActionType = actionType;
            TypeDescription = typeDescription;
        }
    }
}
