using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    internal static class Platform
    {
        private static EOperatingSystem? _operatingSystem = null;

        public static EOperatingSystem OperatingSystem
        {
            get
            {
                if (!_operatingSystem.HasValue)
                    _operatingSystem = DetermineOperatingSystem();

                return _operatingSystem ?? EOperatingSystem.Ten;
            }
        }

        private static EOperatingSystem? DetermineOperatingSystem()
        {
            if (Environment.OSVersion.Version.Major < 6)
                return EOperatingSystem.XP; //Close enough

            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 0)
                return EOperatingSystem.Vista;

            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1)
                return EOperatingSystem.Seven;

            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 2)
                return EOperatingSystem.Eight;

            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 3)
                return EOperatingSystem.EightOne;

            //Assume 10 or newer?
            return EOperatingSystem.Ten;
        }
    }

    internal enum EOperatingSystem
    {
        XP,
        Vista,
        Seven,
        Eight,
        EightOne,
        Ten
    }
}
