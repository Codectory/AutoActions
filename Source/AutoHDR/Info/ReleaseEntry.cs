using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Info
{
    public class ReleaseEntry : BaseViewModel
    {
        private DateTime dateTime;

        public DateTime date
        {
            get { return dateTime; }
            set { dateTime = value; }
        }



    }
}
