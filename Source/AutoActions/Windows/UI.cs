using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Windows
{
    public enum WindowsTheme
    {
        Light,
        Dark
    }
    public static class UI
    {
		private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

		private const string RegistryValueName = "AppsUseLightTheme";
		public static WindowsTheme GetWindowsTheme()
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
			{
				object registryValueObject = key?.GetValue(RegistryValueName);
				if (registryValueObject == null)
				{
					return WindowsTheme.Light;
				}

				int registryValue = (int)registryValueObject;

				return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
			}
		}
	}
}
