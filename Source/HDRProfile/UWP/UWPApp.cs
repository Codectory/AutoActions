using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace HDRProfile.UWP
{
    public class UWPApp
    {
        public string Name { get; private set; } = string.Empty;
        public string Executable { get; private set; } = string.Empty;
        public string InstallLocation { get; private set; } = string.Empty;


        public UWPApp(string installLocation)
        {
            InstallLocation = installLocation;
            ReadAppxManifest();
        }

        private void ReadAppxManifest()
        {


            string appxManifestPath = Path.Combine(InstallLocation, "AppxManifest.xml");
            Tools.Logs.Add($"Retrieving data of UWP app ({appxManifestPath})",false);
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
                Tools.Logs.Add($"Error while  retrieving UWP app ({appxManifestPath})\r\n\r\nContent: {manifestContent}.\r\n\r\n Exception: {ex}",false);
            }
        }

        public override string ToString()
        {
            return $"{Name} {Executable} {InstallLocation}";
        }
    }
}
