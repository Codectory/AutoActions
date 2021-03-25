using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AutoHDR.UWP
{
    public class UWPApp
    {
        public string Name { get; private set; } = string.Empty;
        public string Executable { get; private set; } = string.Empty;
        public string InstallLocation { get; private set; } = string.Empty;

        public string StartArguments { get; private set; } = string.Empty;




        public UWPApp(string installLocation, bool isBundle)
        {
            InstallLocation = installLocation;
            ReadAppxManifest(isBundle);
        }

        private void ReadAppxManifest(bool isBundle)
        {
            string appxManifestPath;
            if (isBundle)
            {
                appxManifestPath = @"AppxMetadata\AppxBundleManifest.xml";
            }
            else
            {
                appxManifestPath = "AppxManifest.xml";
            }
            appxManifestPath = Path.Combine(InstallLocation, appxManifestPath);
            Tools.Logs.Add($"Retrieving data of UWP app ({appxManifestPath})", false);
            try
            {
                using (StreamReader reader = new StreamReader(appxManifestPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppxManifest));
                    AppxManifest appxManifest = (AppxManifest)serializer.Deserialize(reader);
                    Name = ((XmlNode[])appxManifest.Properties.DisplayName)[0].Value;
                    Executable = string.Empty;
                    if (appxManifest.Applications != null && appxManifest.Applications.Application != null)
                        Executable = appxManifest.Applications.Application.Executable;
                }
            }
            catch (Exception ex)
            {
                string manifestContent = string.Empty;
                if (File.Exists(appxManifestPath))
                    manifestContent = File.ReadAllText(appxManifestPath);
                Tools.Logs.AddException($"Error while  retrieving UWP app ({appxManifestPath})\r\n\r\nContent: {manifestContent}.", ex);
            }
        }

        public override string ToString()
        {
            return $"{Name} {Executable} {InstallLocation}";
        }
    }
}
