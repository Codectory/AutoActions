using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Actions
{

    public class DisplayAction : IProfileAction
    {

        public Displays.Display Display { get; private set; } = null;
        public string LocalizedName => ProjectResources.Locale_Texts.Action_HDRSwitch;

        public bool SetHDR = false;
        public bool SetResolution = false;
        public bool ActiveHDR = false;
        public Size Resolution;

        public DisplayAction(Displays.Display display = null)
        {
            Display = display;
            Resolution = display.Resolution;
        }

        public ActionEndResult RunAction()
        {
            try
            {
                if (SetHDR)
                    Displays.HDRController.SetHDRState(Display.UID, ActiveHDR);
                if (SetResolution)
                    Display.SetResolution(Resolution);
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                return new ActionEndResult(false, ex.Message, ex);
            }
        }

    }
}
