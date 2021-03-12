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
        public string Name { get; private set; }
        public string Executable { get; private set; }
        public string InstallLocation { get; private set; }


        public UWPApp(string installLocation)
        {
            InstallLocation = installLocation;
            ReadAppxManifest();
        }

        private void ReadAppxManifest()
        {
            string appxManifestPath = Path.Combine(InstallLocation, "AppxManifest.xml");
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

        public override string ToString()
        {
            return $"{Name} {Executable} {InstallLocation}";
        }
    }
}
