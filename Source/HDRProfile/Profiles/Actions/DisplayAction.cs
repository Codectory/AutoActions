using AutoHDR.ProjectResources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Profiles.Actions
{

    public class DisplayAction : BaseProfileAction
    {
        public List<Displays.Display> AllDisplays => AutoHDR.Displays.DisplayManager.GetActiveMonitors();

        private Displays.Display _display = null;
        public Displays.Display Display { get => _display; set { _display = value;  OnPropertyChanged(); Resolution = _display.Resolution;  } }
        public override string ActionTypeName => ProjectResources.Locale_Texts.DisplayAction;


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

        public override string ActionName
        {
            get
            {
                string returnValue = $"[{ActionTypeName} {Display.Name}]:";
                if (SetHDR)
                    returnValue += $" {Locale_Texts.HDR} {(EnableHDR ? Locale_Texts.Yes : Locale_Texts.No)}";
                if (SetResolution)
                    returnValue += $" {Locale_Texts.Resolution} {Resolution.Width}x{Resolution.Height}";
                if (SetRefreshRate)
                    returnValue += $" {Locale_Texts.RefreshRate} 0Hz";
                return returnValue;
            }
        }


        public DisplayAction()
        {
        }

        public override ActionEndResult RunAction(params object[] parameters)
        {
            try
            {
                if (SetHDR)
                    Displays.HDRController.SetHDRState(Display.UID, EnableHDR);
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
