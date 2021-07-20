using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Profiles.Actions
{

    public class ApplicationAction : BaseProfileAction
    {

        public Displays.Display Display { get; private set; } = null;
        public override string LocalizedName => ProjectResources.Locale_Texts.Action_HDRSwitch;


        private bool _restartApplication = false;
        public bool RestartApplication { get => _restartApplication; set { _restartApplication = value; OnPropertyChanged(); } }

        public ApplicationItem Application { get; }

        public ApplicationAction(ApplicationItem application)
        {
            Application = application;
        }

        public override ActionEndResult RunAction()
        {
            try
            {
                if (RestartApplication)
                    Application.Restart();
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                return new ActionEndResult(false, ex.Message, ex);
            }
        }
    }
}
