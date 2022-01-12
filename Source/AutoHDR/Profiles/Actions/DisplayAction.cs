
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
        public override bool CanSave => (!ChangeResolution || Resolution != null) && (!ChangeRefreshRate || RefreshRate != 0) && (!ChangeColorDepth || ColorDepth !=  ColorDepth.BPCUnkown);
        public override string CannotSaveMessage => ProjectLocales.MessageInvalidSettings;
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

        private uint _displayUID = uint.MaxValue;

        [JsonProperty]
        public uint DisplayUID
        {
            get => _displayUID;
            set
            {
                _displayUID = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Display));
                try
                {
                    Resolution = Display.Resolution;
                    RefreshRate = Display.RefreshRate;
                }
                catch (Exception)
                { }

            }
        }

        private Display _display = null;

        public Display Display 
        { 
            get => AllDisplays.FirstOrDefault(d => d.UID.Equals(DisplayUID));
            set
            {
                DisplayUID = value.UID;

            }
        }

        public override string ActionTypeName => ProjectResources.ProjectLocales.DisplayAction;
      

        private bool _changeHDR = false;
        [JsonProperty]
        public bool ChangeHDR { get => _changeHDR; set { _changeHDR = value; OnPropertyChanged(); } }

        private bool _changeResolution = false;

        [JsonProperty]
        public bool ChangeResolution { get => _changeResolution; set { _changeResolution = value; OnPropertyChanged(); } }

        private bool _changeRefreshRate = false;

        [JsonProperty]
        public bool ChangeRefreshRate { get => _changeRefreshRate; set { _changeRefreshRate = value; OnPropertyChanged(); } }


        private bool _changeColorDepth =false;
        [JsonProperty]
        public bool ChangeColorDepth { get => _changeColorDepth; set { _changeColorDepth = value; OnPropertyChanged(); } }

        private bool _enableHDR = false;

        [JsonProperty]
        public bool EnableHDR { get => _enableHDR; set { _enableHDR = value; OnPropertyChanged(); } }

        private Size _resolution;

        [JsonProperty]
        public Size Resolution 
        {
            get
            {
               return _resolution;
            }
            set { _resolution = value; OnPropertyChanged(); } 
        }

        private int _refreshRate;

        [JsonProperty]
        public int RefreshRate 
        {
            get
            {
                return _refreshRate;
            }
            set { _refreshRate = value; OnPropertyChanged(); } }

        ColorDepth _colorDepth;

        [JsonProperty]
        public ColorDepth ColorDepth
        {
            get
            { 
                return _colorDepth;
            }
            set { _colorDepth = value; OnPropertyChanged(); } }

        public IEnumerable<ColorDepth> ColorDepthValues { get => Enum.GetValues(typeof(ColorDepth)).Cast<ColorDepth>(); }

        public override string ActionDescription
        {
            get
            {
                string returnValue = $"[{Display.Name}]:";
                if (ChangeHDR)
                    returnValue += $" {ProjectLocales.HDR} {(EnableHDR ? ProjectLocales.Yes : ProjectLocales.No)}";
                if (ChangeResolution)
                    returnValue += $" {ProjectLocales.Resolution} {Resolution.Width}x{Resolution.Height}";
                if (ChangeRefreshRate)
                    returnValue += $" {ProjectLocales.RefreshRate} {RefreshRate}Hz";
                if (ChangeColorDepth)
                    returnValue += $" {ProjectLocales.ColorDepth} {ColorDepth}";
                return returnValue;
            }
        }

        public bool ColorDepthIsSupported => DisplayManagerHandler.Instance.GraphicsCardType == GraphicsCardType.NVIDIA;


        public DisplayAction() : base()
        {
            _display = Display.AllDisplays;
        }

        public override ActionEndResult RunAction(ApplicationChangedType applicationChangedType)
        {
            try
            {
                if (ChangeHDR)
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
                if (ChangeResolution)
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
                if (ChangeRefreshRate)
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
                if (ChangeColorDepth)
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
