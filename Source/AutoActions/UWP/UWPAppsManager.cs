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

namespace AutoActions.UWP
{
    public static class UWPAppsManager
    {
        static PackageManager manager = new PackageManager();


        private const string xboxPassAppFN = "Microsoft.GamingApp_8wekyb3d8bbwe";


        public static UWPApp GetUWPApp(string packageNameOrFamilyPackageName, string applicationID = "")
        {
            var package = manager.FindPackageForUser(WindowsIdentity.GetCurrent().User.Value, packageNameOrFamilyPackageName);
            if (package == null)
                return GetUWPAppCompatible(packageNameOrFamilyPackageName, applicationID);
            return new UWPApp(package);
        }

        private static UWPApp GetUWPAppCompatible(string familyPackageName, string applicationID)
        {
            foreach (var package in manager.FindPackagesForUser(WindowsIdentity.GetCurrent().User.Value))
            {
                try
                {
                    UWPApp uwpApp = new UWPApp(package);
                    if (uwpApp.FamilyPackageName.Equals(familyPackageName) && uwpApp.ApplicationID.Equals(applicationID))
                        return uwpApp;
                }
                catch {}
            }
            return null;
        }


        public static List<ApplicationItem> GetUWPApps()
        {

            Globals.Logs.Add($"Retrieving UWP apps...", false);

            List<ApplicationItem> uwpApps = new List<ApplicationItem>();
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
                        UWPApp uwpApp = new UWPApp(package);
                        if (!string.IsNullOrEmpty(uwpApp.ApplicationID))
                            uwpApps.Add(new ApplicationItem(uwpApp));
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
