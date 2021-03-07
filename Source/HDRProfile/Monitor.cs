using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDRProfile
{
    public class Monitor : BaseViewModel
    {
        private bool _autoHDR = true;

        public bool AutoHDR { get => _autoHDR; set { _autoHDR = value; OnPropertyChanged(); } }


        private string _name;
        public string Name { get => _name;  set { _name = value; OnPropertyChanged(); } }

        private string _caption;
        public string Caption { get => _caption;  set { _caption = value; OnPropertyChanged(); } }

        private string _deviceID;
        public string DeviceID { get => _deviceID;  set { _deviceID = value; OnPropertyChanged(); } }

        private UInt32 _uid;
        public UInt32 UID { get => _uid;  set { _uid = value; OnPropertyChanged(); } }

        private bool _hdrState;

        public bool HDRState { get => _hdrState; set { _hdrState = value; OnPropertyChanged(); } }

        public Monitor(string name, string caption, string deviceID, UInt32 uid)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Caption = caption ?? throw new ArgumentNullException(nameof(caption));
            DeviceID = deviceID ?? throw new ArgumentNullException(nameof(deviceID));
            UID = uid;
            UpdateHDRState();
        }

        private Monitor()
        {


        }

        public void UpdateHDRState()
        {
            HDRState= HDRController.GetHDRState(UID);
        }


    }
}
