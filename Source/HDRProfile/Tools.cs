using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace HDRProfile
{
    public static class Tools
    {
        public static void SetAutoStart(string applicationName, string filePath, bool autostart)
        {
            RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
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

        public static IDictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
        (Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            }
            return ret;
        }
    }
}

