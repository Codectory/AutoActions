//using Microsoft.Win32;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace HDRProfile
//{
//    class NightLightManager
//    {

//        private const string NightLightRegistryPath = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\CloudStore\Store\DefaultAccount\Current\{0}";
//        private const string StatePath = @"default$windows.data.bluelightreduction.bluelightreductionstate\windows.data.bluelightreduction.bluelightreductionstate";
//        private const string SettingsPath = @"default$windows.data.bluelightreduction.settings\windows.data.bluelightreduction.settings";



//        public static readonly byte[] Off = new byte[] { 67, 66, 1, 0, 10, 2, 1, 0, 42, 6, 204, 130, 148, 130, 6, 42, 43, 14, 19, 67, 66, 1, 0, 208, 10, 2, 198, 20, 175, 171, 145, 221, 130, 238, 196,235,1,0,0,0,0 };
//        public static readonly byte[] On = new byte[] { 2, 0, 0, 0, 128, 208, 150, 171, 186, 109, 210, 1, 0, 0, 0, 0, 67, 66, 1, 0, 16, 0, 208, 10, 2, 198, 20, 221, 137, 219, 220, 170, 183, 155, 233, 1, 0 };
//        public static readonly byte[] AutoOn = new byte[] { 2, 0, 0, 0, 89, 63, 239, 213, 232, 109, 210, 1, 0, 0, 0, 0, 67, 66, 1, 0, 2, 1, 202, 20, 14, 21, 0, 202, 30, 14, 7, 0, 207, 40, 188, 62, 202, 50, 14, 16, 46, 54, 0, 202, 60, 14, 8, 46, 46, 0, 0 };
//        public static readonly byte[] AutoOff = new byte[] { 2, 0, 0, 0, 175, 164, 252, 55, 235, 109, 210, 1, 0, 0, 0, 0, 67, 66, 1, 0, 202, 20, 14, 21, 0, 202, 30, 14, 7, 0, 207, 40, 188, 62, 202, 50, 14, 16, 46, 54, 0, 202, 60, 14, 8, 46, 46, 0, 0 };
//        public static readonly byte[] NoShift = new byte[] { 2, 0, 0, 0, 255, 124, 43, 3, 82, 107, 210, 1, 0, 0, 0, 0, 67, 66, 1, 0, 2, 1, 202, 20, 14, 21, 0, 202, 30, 14, 7, 0, 207, 40, 168, 70, 202, 50, 14, 16, 46, 49, 0, 202, 60, 14, 8, 46, 47, 0, 0 };
//        public static readonly byte[] MinimumShift = new byte[] { 2, 0, 0, 0, 224, 193, 179, 114, 82, 107, 210, 1, 0, 0, 0, 0, 67, 66, 1, 0, 2, 1, 202, 20, 14, 21, 0, 202, 30, 14, 7, 0, 207, 40, 236, 57, 202, 50, 14, 16, 46, 49, 0, 202, 60, 14, 8, 46, 47, 0, 0 };
//        public static readonly byte[] MediumShift = new byte[] { 2, 0, 0, 0, 49, 229, 185, 33, 82, 107, 210, 1, 0, 0, 0, 0, 67, 66, 1, 0, 2, 1, 202, 20, 14, 21, 0, 202, 30, 14, 7, 0, 207, 40, 200, 42, 202, 50, 14, 16, 46, 49, 0, 202, 60, 14, 8, 46, 47, 0, 0 };
//        public static readonly byte[] LargeShift = new byte[] { 2, 0, 0, 0, 22, 255, 5, 128, 82, 107, 210, 1, 0, 0, 0, 0, 67, 66, 1, 0, 2, 1, 202, 20, 14, 21, 0, 202, 30, 14, 7, 0, 207, 40, 138, 27, 202, 50, 14, 16, 46, 49, 0, 202, 60, 14, 8, 46, 47, 0, 0 };
//        public static readonly byte[] MaximumShift = new byte[] { 2, 0, 0, 0, 199, 91, 231, 198, 81, 107, 210, 1, 0, 0, 0, 0, 67, 66, 1, 0, 2, 1, 202, 20, 14, 21, 0, 202, 30, 14, 7, 0, 207, 40, 208, 15, 202, 50, 14, 16, 46, 49, 0, 202, 60, 14, 8, 46, 47, 0, 0 };


//        public void SetNightLightState(bool activate)
//        {
//            string registry = string.Format(NightLightRegistryPath, StatePath);

//            if (activate)
//            {
//                Registry.SetValue(registry, "Data", On);
//            }

//            else
//            {
//                Registry.SetValue(registry, "Data", Off);

//            }
//        }

//        public bool GetNightLightState()
//        {
//            string registry = string.Format(NightLightRegistryPath, StatePath);
//            byte[] value = (byte[])Registry.GetValue(registry, "Data", Off);
//            return On.Equals(value);
//        }


//        public void SetNightLightAutomation(bool activate)
//        {
//            string registry = string.Format(NightLightRegistryPath, SettingsPath);

//            if (activate)
//            {
//                Registry.SetValue(registry, "Data", AutoOn);
//            }

//            else
//            {
//                Registry.SetValue(registry, "Data", AutoOff);

//            }
//        }


//        public bool GetNightLightAutomation()
//        {
//            string registry = string.Format(NightLightRegistryPath, SettingsPath);
//            return On.Equals(Registry.GetValue(registry, "Data", Off));
//        }
//    }
//}
