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
                if (value.IsAllDisplay())
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


        private bool _setColorDepth =false;
        [JsonProperty]
        public bool SetColorDepth { get => _setColorDepth; set { _setColorDepth = value; OnPropertyChanged(); } }

        private bool _enableHDR = false;

        [JsonProperty]
        public bool EnableHDR { get => _enableHDR; set { _enableHDR = value; OnPropertyChanged(); } }

        private Size _resolution;

        [JsonProperty]
        public Size Resolution { get => _resolution; set { _resolution = value; OnPropertyChanged(); } }

        private int _refreshRate;

        [JsonProperty]
        public int RefreshRate { get => _refreshRate; set { _refreshRate = value; OnPropertyChanged(); } }
        
        int _colorDepth;

        [JsonProperty]
        public int ColorDepth { get => _colorDepth; set { _colorDepth = value; OnPropertyChanged(); } }

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
                    returnValue += $" {Locale_Texts.RefreshRate} {RefreshRate}Hz";
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
                    if (Display.IsAllDisplay())
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"{(EnableHDR ? "Activating" : "Deactivating")} HDR for all displays."));
                        if (EnableHDR)
                            DisplayManager.Instance.ActivateHDR();
                        else
                            DisplayManager.Instance.DeactivateHDR();
                    }
                    else
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"{(EnableHDR ? "Activating" : "Deactivating")} HDR for display {Display.Name}"));
                        Displays.HDRController.SetHDRState(Display.UID, EnableHDR);
                    }
                System.Threading.Thread.Sleep(100);
                if (SetResolution)
                    if (Display.IsAllDisplay())
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting resolution {Resolution} for all displays."));
                        foreach (Displays.Display display in AutoHDR.Displays.DisplayManager.GetActiveMonitors())
                            display.SetResolution(Resolution);
                    }
                    else
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting resolution {Resolution} for display {Display.Name}"));
                        Display.SetResolution(Resolution);
                    }
                System.Threading.Thread.Sleep(100);
                if (SetRefreshRate)
                    if (Display.IsAllDisplay())
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting refresh rate {RefreshRate} for all displays."));

                        foreach (Displays.Display display in AutoHDR.Displays.DisplayManager.GetActiveMonitors())
                            display.SetRefreshRate(RefreshRate);
                    }
                    else
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting refresh rate {RefreshRate} for display {Display.Name}"));
                        Display.SetRefreshRate(RefreshRate);
                    }
                System.Threading.Thread.Sleep(100);
                if (SetColorDepth)
                    if (Display.IsAllDisplay())
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting color depth {ColorDepth} for all displays."));

                        foreach (Displays.Display display in AutoHDR.Displays.DisplayManager.GetActiveMonitors())
                            display.SetColorDepth(ColorDepth);
                    }
                    else
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting color depth {ColorDepth} for display {Display.Name}"));
                        Display.SetColorDepth(RefreshRate);
                    }
                System.Threading.Thread.Sleep(100);
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                return new ActionEndResult(false, ex.Message, ex);
            }
        }
    }
}
