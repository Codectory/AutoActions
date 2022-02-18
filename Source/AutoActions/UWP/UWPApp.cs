using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.ApplicationModel;

namespace AutoActions.UWP
{
    public class UWPApp
    {
        public string Name { get; private set; } = string.Empty;
        public string Executable { get; private set; } = string.Empty;



        public string ExecutablePath
        {
            get
            {
                if (IsWebApp)
                    return @"C:\Windows\System32\WWAHost.exe";
                else
                    return Path.Combine(InstallLocation, Executable);


            }
        }

        public bool IsWebApp  { get; private set; } = false;
        public string InstallLocation { get; private set; } = string.Empty;
        public string FamilyPackageName { get; private set; } = string.Empty;
        public string FullPackageName { get; private set; } = string.Empty;

        public string ApplicationID { get; private set; } = string.Empty;

        public string Identity { get; private set; } = string.Empty;
        public string IconPath { get; private set; } = string.Empty;


        private UWPApp()
        {

        }

        public UWPApp(Package package)
        {
            ReadAppxManifest(package);
        }

        private void ReadAppxManifest(Package package)
        {
            string appxManifestPath;
            if (package.IsBundle)
            {
                appxManifestPath = @"AppxMetadata\AppxBundleManifest.xml";
            }
            else
            {
                appxManifestPath = "AppxManifest.xml";
            }
            InstallLocation = package.InstalledLocation.Path;
            appxManifestPath = Path.Combine(InstallLocation, appxManifestPath);
            Globals.Logs.Add($"Retrieving data of UWP app ({appxManifestPath})", false);
            try
            {
                using (StreamReader reader = new StreamReader(appxManifestPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppxManifest));
                    AppxManifest appxManifest = (AppxManifest)serializer.Deserialize(reader);
                    Name = package.DisplayName;
                    //Name = ((XmlNode[])appxManifest.Properties.DisplayName)[0].Value;
                    if (Name.Contains("ms-resource:"))
                    {
                        Name = GetNameOfStrangeMicrosoftAppxManifest(appxManifest);
                    }
                    Executable = string.Empty;
                    if (appxManifest.Applications != null && appxManifest.Applications.Application != null)
                        Executable = appxManifest.Applications.Application.Executable;
                    if (Executable == null)
                        IsWebApp = true;
                    FamilyPackageName = package.Id.FamilyName;
                    FullPackageName = package.Id.FullName;
                    var a = package.Id.ResourceId;
                    ApplicationID = appxManifest.Applications?.Application.Id;
                    Identity = appxManifest.Identity.Name;
                    IconPath = GetIconPath(Path.Combine(InstallLocation, ((XmlNode[])(appxManifest.Properties.Logo))[0].Value));
                }
            }
            catch (Exception ex)
            {
                string manifestContent = string.Empty;
                if (File.Exists(appxManifestPath))
                    manifestContent = File.ReadAllText(appxManifestPath);
                Globals.Logs.AddException($"Error while  retrieving UWP app ({appxManifestPath})\r\n\r\nContent: {manifestContent}.", ex);
                throw ex;
            }
        }

        private static string GetIconPath(string iconPath)
        {
            FileInfo fi = new FileInfo(iconPath);
            if (fi.Exists) 
                return fi.FullName;
            string fileName = fi.Name.Replace(fi.Extension, "");
            string scale400 = $"{Path.Combine(fi.Directory.FullName, fi.Directory.FullName, fileName)}.scale-400{fi.Extension}";
            if (File.Exists(scale400))
                return scale400;

            string scale200 = $"{Path.Combine(fi.Directory.FullName, fi.Directory.FullName, fileName)}.scale-200{fi.Extension}";
            if (File.Exists(scale200))
                return scale200;

            string scale150 = $"{Path.Combine(fi.Directory.FullName, fi.Directory.FullName, fileName)}.scale-150{fi.Extension}";
            if (File.Exists(scale150))
                return scale150;
            string scale125 = $"{Path.Combine(fi.Directory.FullName, fi.Directory.FullName, fileName)}.scale-125{fi.Extension}";
            if (File.Exists(scale125))
                return scale125;
            string scale100 = $"{Path.Combine(fi.Directory.FullName, fi.Directory.FullName, fileName)}.scale-100{fi.Extension}";
            if (File.Exists(scale100))
                return scale100;
            return string.Empty;
        }

        private string GetNameOfStrangeMicrosoftAppxManifest(AppxManifest appxManifest)
        {
            string name = appxManifest.Identity.Name;
            name = name.Replace("Microsoft.", "");
            string newName = string.Empty;
            for (int i = 0; i < name.Length; i++)
            {
                if (i == 0)
                {
                    newName += name[i];
                }
                else if (char.IsUpper(name[i]))
                    newName += $" {name[i]}";
                else
                    newName += name[i];
            }
            return newName;

        }

        public override string ToString()
        {
            return $"{Name} {Executable} {InstallLocation}";
        }
    }
}
