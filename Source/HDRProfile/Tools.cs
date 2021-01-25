using Microsoft.Win32;

namespace HDRProfile
{
    public static class Tools
    {
        public static void SetAutoStart(string applicationName, string filePath, bool autostart)
        {
            RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            object existing = rk.GetValue(applicationName);
            if (filePath.Equals(existing) && autostart)
                return;
            if (rk.GetValue(applicationName) == null && !autostart)
                return;

            if (autostart)
                rk.SetValue(applicationName, filePath);
            else
                rk.DeleteValue(applicationName, false);
        }
    }
}

