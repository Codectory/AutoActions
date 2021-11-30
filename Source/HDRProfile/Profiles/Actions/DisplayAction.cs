using AutoHDR.Displays;
using AutoHDR.ProjectResources;
using CodectoryCore.UI.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AutoHDR.Profiles.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DisplayAction : ProfileActionBase
    {
        public List<Displays.Display> AllDisplays
        {
            get
            {
                List<Displays.Display> displays = new List<Displays.Display>();
                displays.Add(Displays.Display.AllDisplays);
                displays.AddRange(AutoHDR.Displays.DisplayManager.GetActiveMonitors());
                return displays;
            }
        }
        private Displays.Display _display = null;

        [JsonProperty]
        public Displays.Display Display { 
            get => _display; 
            set 
            { 
                _display = value; 
                OnPropertyChanged();
                if (value.Equals(Displays.Display.AllDisplays))
                {
                    Resolution = AllDisplays[1].Resolution;
                    RefreshRate = AllDisplays[1].RefreshRate;
                }
                else
                {
                    Resolution = value.Resolution;
                    RefreshRate = value.RefreshRate;
                }
            } 
        }
        public override string ActionTypeName => ProjectResources.Locale_Texts.DisplayAction;

       

        private bool _setHDR = false;
        [JsonProperty]
        public bool SetHDR { get => _setHDR; set { _setHDR = value; OnPropertyChanged(); } }

        private bool _setResolution = false;

        [JsonProperty]
        public bool SetResolution { get => _setResolution; set { _setResolution = value; OnPropertyChanged(); } }

        private bool _setRefreshRate = false;

        [JsonProperty]
        public bool SetRefreshRate { get => _setRefreshRate; set { _setRefreshRate = value; OnPropertyChanged(); } }

        private bool _enableHDR = false;

        [JsonProperty]
        public bool EnableHDR { get => _enableHDR; set { _enableHDR = value; OnPropertyChanged(); } }

        private Size _resolution;

        [JsonProperty]
        public Size Resolution { get => _resolution; set { _resolution = value; OnPropertyChanged(); } }

        private int _refreshRate;

        [JsonProperty]
        public int RefreshRate { get => _refreshRate; set { _refreshRate = value; OnPropertyChanged(); } }

        public override string ActionDescription
        {
            get
            {
                string returnValue = $"[{Display.Name}]:";
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
                    if (Display.Equals(Displays.Display.AllDisplays))
                        if (EnableHDR)
                            DisplayManager.Instance.ActivateHDR();
                        else
                            DisplayManager.Instance.DeactivateHDR();
                    else
                        Displays.HDRController.SetHDRState(Display.UID, EnableHDR);
                if (SetResolution)
                    if (Display.Equals(Displays.Display.AllDisplays))
                    {
                        foreach (Displays.Display display in AutoHDR.Displays.DisplayManager.GetActiveMonitors())
                            display.SetResolution(Resolution);
                    }
                    else
                        Display.SetResolution(Resolution);

                if (SetRefreshRate)
                    if (Display.Equals(Displays.Display.AllDisplays))
                    {
                        foreach (Displays.Display display in AutoHDR.Displays.DisplayManager.GetActiveMonitors())
                            display.SetRefreshRate(RefreshRate);
                    }
                    else
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
