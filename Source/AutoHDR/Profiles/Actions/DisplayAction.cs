
using AutoHDR.Displays;
using AutoHDR.ProjectResources;
using CodectoryCore.UI.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AutoHDR.Profiles.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DisplayAction : ProfileActionBase
    {
        public List<Display> AllDisplays
        {
            get
            {
                List<Display> displays = new List<Display>();
                displays.Add(Display.AllDisplays);
                displays.AddRange(DisplayManagerHandler.Instance.GetActiveMonitors());
                return displays;
            }
        }
        private Display _display = null;

        [JsonProperty]
        public Display Display { 
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
        public override string ActionTypeName => ProjectResources.ProjectLocales.DisplayAction;

       

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

        ColorDepth _colorDepth;

        [JsonProperty]
        public ColorDepth ColorDepth { get => _colorDepth; set { _colorDepth = value; OnPropertyChanged(); } }

        public IEnumerable<ColorDepth> ColorDepthValues { get => Enum.GetValues(typeof(ColorDepth)).Cast<ColorDepth>(); }

        public override string ActionDescription
        {
            get
            {
                string returnValue = $"[{Display.Name}]:";
                if (SetHDR)
                    returnValue += $" {ProjectLocales.HDR} {(EnableHDR ? ProjectLocales.Yes : ProjectLocales.No)}";
                if (SetResolution)
                    returnValue += $" {ProjectLocales.Resolution} {Resolution.Width}x{Resolution.Height}";
                if (SetRefreshRate)
                    returnValue += $" {ProjectLocales.RefreshRate} {RefreshRate}Hz";
                if (SetColorDepth)
                    returnValue += $" {ProjectLocales.ColorDepth} {ColorDepth}";
                return returnValue;
            }
        }

        public bool ColorDepthIsSupported => DisplayManagerHandler.Instance.GraphicsCardType == GraphicsCardType.NVIDIA;


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
                            DisplayManagerHandler.Instance.ActivateHDR();
                        else
                            DisplayManagerHandler.Instance.DeactivateHDR();
                    }
                    else
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"{(EnableHDR ? "Activating" : "Deactivating")} HDR for display {Display.Name}"));
                        HDRController.SetHDRState(Display.UID, EnableHDR);
                    }
                System.Threading.Thread.Sleep(100);
                if (SetResolution)
                    if (Display.IsAllDisplay())
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting resolution {Resolution} for all displays."));
                        foreach (Display display in DisplayManagerHandler.Instance.GetActiveMonitors())
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

                        foreach (Display display in DisplayManagerHandler.Instance.GetActiveMonitors())
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

                        foreach (Display display in DisplayManagerHandler.Instance.GetActiveMonitors())
                            display.SetColorDepth(ColorDepth);
                    }
                    else
                    {
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Setting color depth {ColorDepth} for display {Display.Name}"));
                        Display.SetColorDepth(ColorDepth);
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
