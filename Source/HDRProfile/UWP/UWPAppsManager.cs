using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Permissions;
using System.Security.Principal;

using Windows.Management.Deployment;
using Windows.ApplicationModel;

namespace AutoHDR.UWP
{
    public static class UWPAppsManager
    {

        private const string xboxPassAppFN = "Microsoft.GamingApp_8wekyb3d8bbwe";


        public static List<ApplicationItem> GetUWPApps()
        {

            Globals.Logs.Add($"Retrieving UWP apps...", false);

            List<ApplicationItem> uwpApps = new List<ApplicationItem>();
            var manager = new PackageManager();
            IEnumerable<Package> packages = manager.FindPackagesForUser(WindowsIdentity.GetCurrent().User.Value);
            try
            {
                foreach (var package in packages)
                {
                    string s = package.DisplayName;
                    if (package.IsFramework || package.IsResourcePackage || package.SignatureKind != PackageSignatureKind.Store )
                    {
                        continue;
                    }

                    try
                    {
                        if (package.InstalledLocation == null)
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    try
                    {
                        uwpApps.Add(new ApplicationItem(new UWPApp(package)));
                  }
                    catch
                    {
                        continue;
                    }
                }
                return uwpApps.OrderBy(u => u.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                Globals.Logs.AddException($"Retrieving UWP apps failed.", ex);
                throw;
            }
        }

        public static void StartUWPApp(string FamilyPackage, string applicationID)
        {
            Process process = new Process();
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = $"shell:AppsFolder\\{FamilyPackage}!{applicationID}";
            process.Start();

        }

    }
}
