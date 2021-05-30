using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Actions
{
    public interface IProfileAction
    {
        string LocalizedName { get; }
        ActionEndResult RunAction();
    }
}
