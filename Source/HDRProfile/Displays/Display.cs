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
        public static readonly Display AllDisplays = new Display(ProjectResources.Locale_Texts.AllDisplays, UInt32.MaxValue, UInt32.MaxValue, new Size(0, 0),0,0);
        private bool _managed = true;

        [JsonProperty]
        public bool Managed { get => _managed; set { _managed = value; OnPropertyChanged(); } }


        private string _name;

        [JsonProperty]
        public string Name { get => _name;  set { _name = value; OnPropertyChanged(); } }


        private UInt32 _uid;

        [JsonProperty]
        public UInt32 UID { get => _uid;  set { _uid = value; OnPropertyChanged(); } }

        private uint _id;

        [JsonProperty]
        public uint ID { get => _id; private set { _id = value; OnPropertyChanged(); } }

        private bool _hdrState;

        public bool HDRState { get => _hdrState; set { _hdrState = value; OnPropertyChanged(); } }



        private Size _resolution;
        public Size Resolution { get => _resolution; set { _resolution = value; OnPropertyChanged(); } }


        private int _refreshRate;

        public int RefreshRate { get => _refreshRate; set { _refreshRate = value; OnPropertyChanged(); } }

        private int _colorDepth;

        public int ColorDepth { get => _colorDepth; set { _colorDepth = value; OnPropertyChanged(); } }

        private Display()
        {


        }

        public Display(string name, uint uID, uint id, Size resolution,  int refreshRate, int colorDepth)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            UID = uID;
            ID = id;
            Resolution = resolution;
            RefreshRate = refreshRate;
            ColorDepth = colorDepth;
        }

        public void UpdateHDRState()
        {
            HDRState= HDRController.GetHDRState(UID);
        }

        internal void SetResolution(Size resolution)
        {
            DisplayManager.Instance.SetResolution(ID, resolution);
        }

        internal void SetRefreshRate(int refreshRate)
        {
            DisplayManager.Instance.SetRefreshRate(ID, refreshRate);
        }

        internal void SetColorDepth(int colorDepth)
        {
            DisplayManager.Instance.SetColorDepth(ID, colorDepth);
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


        public override int GetHashCode()
        {
            int hashCode = -254808592;
            hashCode = hashCode * -1521134295 + Managed.GetHashCode();
            hashCode = hashCode * -1521134295 + UID.GetHashCode();
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            hashCode = hashCode * -1521134295 + _hdrState.GetHashCode();
            hashCode = hashCode * -1521134295 + HDRState.GetHashCode();
            hashCode = hashCode * -1521134295 + _resolution.GetHashCode();
            hashCode = hashCode * -1521134295 + Resolution.GetHashCode();
            hashCode = hashCode * -1521134295 + _refreshRate.GetHashCode();
            hashCode = hashCode * -1521134295 + RefreshRate.GetHashCode();
            return hashCode;
        }
    }
}
