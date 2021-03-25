using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace AutoHDR.UWP
{
    public static class UWPAppsManager
    {

        private const string xboxPassAppFN = "Microsoft.GamingApp_8wekyb3d8bbwe";


        public static List<ApplicationItem> GetUWPApps()
        {

            Tools.Logs.Add($"Retrieving UWP apps...", false);

            List<ApplicationItem> uwpApps = new List<ApplicationItem>();
            var manager = new PackageManager();
            IEnumerable<Package> packages = manager.FindPackagesForUser(WindowsIdentity.GetCurrent().User.Value);
            try
            {
                foreach (var package in packages)
                {
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
                        string manifestPath;
                        if (package.IsBundle)
                        {
                            manifestPath = @"AppxMetadata\AppxBundleManifest.xml";
                        }
                        else
                        {
                            manifestPath = "AppxManifest.xml";
                        }
                        manifestPath = Path.Combine(package.InstalledLocation.Path, manifestPath);

                        UWPApp uwpApp = new UWPApp(package.InstalledLocation.Path, package.IsBundle);
                        if (!string.IsNullOrEmpty(uwpApp.Executable) && !uwpApp.Name.Contains("ms-resource:"))
                            uwpApps.Add(new ApplicationItem(uwpApp.Name, Path.Combine(uwpApp.InstallLocation, uwpApp.Executable)) { IsUWP = true});
                    }
                    catch
                    {
                        continue;
                    }
                }
                return uwpApps;
            }
            catch (Exception ex)
            {
                Tools.Logs.AddException($"Retrieving UWP apps failed.", ex);
                throw;
            }
        }

    }
}
