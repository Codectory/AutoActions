using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Profiles.Actions
{

    public class DisplayAction : BaseProfileAction
    {

        public Displays.Display Display { get; private set; } = null;
        public override string LocalizedName => ProjectResources.Locale_Texts.Action_HDRSwitch;


        private bool _setHDR = false;
        public bool SetHDR { get => _setHDR; set { _setHDR = value; OnPropertyChanged(); } }

        public bool _setResolution = false;
        public bool SetResolution { get => _setResolution; set { _setResolution = value; OnPropertyChanged(); } }

        public bool _setRefreshRate = false;
        public bool SetRefreshRate { get => _setRefreshRate; set { _setRefreshRate = value; OnPropertyChanged(); } }

        public bool _enableHDR = false;
        public bool EnableHDR { get => _enableHDR; set { _enableHDR = value; OnPropertyChanged(); } }

        public Size _resolution;
        public Size Resolution { get => _resolution; set { _resolution = value; OnPropertyChanged(); } }

        public int _refreshRate;
        public int RefreshRate { get => _refreshRate; set { _refreshRate = value; OnPropertyChanged(); } }


        public DisplayAction(Displays.Display display = null)
        {
            Display = display;
            Resolution = display.Resolution;
        }

        public override ActionEndResult RunAction()
        {
            try
            {
                if (SetHDR)
                    Displays.HDRController.SetHDRState(Display.UID, EnableHDR);
                if (SetResolution)
                    Display.SetResolution(Resolution);
                if (SetRefreshRate)
                    Display.SetRefreshRate(RefreshRate);
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                return new ActionEndResult(false, ex.Message, ex);
            }
        }

    }
}
