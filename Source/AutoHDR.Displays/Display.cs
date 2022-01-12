using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AutoHDR.Displays
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Display : BaseViewModel
    {
        public static readonly Display AllDisplays = new Display(Locale.AllDisplays, UInt32.MaxValue, UInt32.MaxValue);
        private bool _managed = true;

        [JsonProperty]
        public bool Managed { get => _managed; set { _managed = value; OnPropertyChanged(); } }


        private bool _isPrimary;

        public bool IsPrimary { get => _isPrimary; set { _isPrimary = value; OnPropertyChanged(); } }

        private string _name;

        public string Name { get => _name;  set { _name = value; OnPropertyChanged(); } }

        private string _graphicsCard;

        public string GraphicsCard { get => _graphicsCard; set { _graphicsCard = value; OnPropertyChanged(); } }


        private UInt32 _uid;

        [JsonProperty]
        public UInt32 UID 
        { 
            get => _uid;  
            set { _uid = value; OnPropertyChanged(); }
        }

        private uint _id;

        [JsonProperty]
        public uint ID { get => _id;  set { _id = value; OnPropertyChanged(); } }

        private bool _hdrState;

        public bool HDRState { get => _hdrState; set { _hdrState = value; OnPropertyChanged(); } }



        private Size _resolution;
        public Size Resolution { get => _resolution; set { _resolution = value; OnPropertyChanged(); } }


        private int _refreshRate;

        public int RefreshRate { get => _refreshRate; set { _refreshRate = value; OnPropertyChanged(); } }

        private ColorDepth _colorDepth;

        public ColorDepth ColorDepth { get => _colorDepth; set { _colorDepth = value; OnPropertyChanged(); } }

        public object Tag;

        private Display()
        {


        }

        public Display(DisplayInformation monitorInformation, uint uid)
        {
            Name = monitorInformation.DisplayDevice.DeviceName;
            UID = uid;
            ID = monitorInformation.Id;
            IsPrimary = monitorInformation.IsPrimary;
            Resolution = new Size(monitorInformation.Devmode.dmPelsWidth, monitorInformation.Devmode.dmPelsHeight);
            RefreshRate = monitorInformation.Devmode.dmDisplayFrequency;
            GraphicsCard = monitorInformation.DisplayDevice.DeviceString;
        }

        public Display(string name, uint uid, uint id)
        {
            Name = name;
            UID = uid;
            ID = id;
        }


        public void UpdateHDRState()
        {
            HDRState= HDRController.GetHDRState(UID);
        }

        public void SetResolution(Size resolution)
        {
            DisplayManagerHandler.Instance.SetResolution(this, resolution);
        }

        public void SetRefreshRate(int refreshRate)
        {
            DisplayManagerHandler.Instance.SetRefreshRate(this, refreshRate);
        }

        public void SetColorDepth(ColorDepth colorDepth)
        {
            DisplayManagerHandler.Instance.SetColorDepth(this, colorDepth);
        }

        public bool IsAllDisplay()
        {
            return UID.Equals(AllDisplays.UID);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType().Equals(typeof(Display)))
            {
                Display display = (Display)obj;
                return
                   Managed == display.Managed &&
                   UID == display.UID &&
                   ID == display.ID &&
                   _hdrState == display._hdrState &&
                   HDRState == display.HDRState &&
                   EqualityComparer<Size>.Default.Equals(_resolution, display._resolution) &&
                   EqualityComparer<Size>.Default.Equals(Resolution, display.Resolution) &&
                   _refreshRate == display._refreshRate &&
                   RefreshRate == display.RefreshRate;
            }
            else 
                return false;
          
        }
    }
}
