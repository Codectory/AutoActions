using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace HDRProfile.UWP
{
    public static class UWPAppsManager
    {

        public static List<ApplicationItem> GetUWPApps()
        {

            PowerShell powerShell = PowerShell.Create();
            string powerShellScript = "Get-AppxPackage | Where-Object {$_.SignatureKind -eq'Store'} | Where-Object {$_.IsFramework -eq $false} | where-Object {$_.NonRemovable -eq $false}";
            powerShell.AddScript(powerShellScript);
            Tools.Logs.Add($"Powershell query: {powerShellScript}", false);
            try
            {
                Collection<PSObject> results = powerShell.Invoke();
                Tools.Logs.Add($"Found {results.Count} PSObjects.", false);
                List<ApplicationItem> uwpApps = new List<ApplicationItem>();
                foreach (PSObject psObject in results)
                {
                    UWPApp uwpApp = new UWPApp(psObject.Properties["InstallLocation"].Value.ToString());
                    if (!string.IsNullOrEmpty(uwpApp.Executable) && !uwpApp.Name.Contains("ms-resource:"))
                        uwpApps.Add(new ApplicationItem(uwpApp.Name, Path.Combine(uwpApp.InstallLocation, uwpApp.Executable)));
                }
                return uwpApps;
            }
            catch (Exception ex)
            {
                Tools.Logs.Add($"Powershell query failed. {ex}", false);

                throw;
            }
        }
    }
}
