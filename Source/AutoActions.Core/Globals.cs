using CodectoryCore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Core
{
    public class Globals
    {
        public static Logs Logs = new Logs($"{System.AppDomain.CurrentDomain.BaseDirectory}AutoActions.log", "AutoActions", Assembly.GetExecutingAssembly().GetName().Version.ToString(), false);

    }
}
