using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Updater
{
    class Program
    {
        static string temporaryFolder;

        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(20000);
            UpdateData updateData = new UpdateData();
            updateData.SaveUpdateData("D:\\UpdateData.json");
            temporaryFolder = GetTemporaryDirectory();
            try
            {
                bool download = bool.Parse(args[0]);
                string zip = GetZip(download, args[1]);
                string targetFolder = args[2];
                string callingProcess = args[3];
                Update(zip, targetFolder, callingProcess);
                Process.Start(Path.Combine(targetFolder, $"{callingProcess}.exe"));

            }


            finally
            {
                Directory.Delete(temporaryFolder, true);
            }
        }


        private static string GetZip(bool download, string path)
        {
            string updateZip = Path.Combine(temporaryFolder, "Update.zip");
            if (download)
            {
                updateZip = Path.Combine(temporaryFolder, "Update.zip");
                using (WebClient myWebClient = new WebClient())
                {
                    // Download the Web resource and save it into the current filesystem folder.
                    myWebClient.DownloadFile(path, updateZip);
                }
            }
            else
                File.Copy(path, updateZip, true);
            return updateZip;
        }

        private static void Update(string zip, string targetFolder, string callingProcess)
        {
            System.Threading.Thread.Sleep(2000);
            ZipFile.ExtractToDirectory(zip, temporaryFolder);
            File.Delete(zip);
            UpdateData updateData = UpdateData.LoadFromFile(Path.Combine(temporaryFolder, "UpdateData.json"));
            Process[] processes  = Process.GetProcessesByName(callingProcess);
            foreach (var process in processes)
                if (process.StartInfo.WorkingDirectory.ToUpperInvariant().Equals(targetFolder.ToUpperInvariant()))
                    process.Kill();
            List<string> filesToCopy = GetAllFiles(temporaryFolder);
            if (filesToCopy.Contains(Path.Combine(temporaryFolder, "UpdateData.json")))
                filesToCopy.Remove(Path.Combine(temporaryFolder, "UpdateData.json"));
            if (filesToCopy.Contains(Path.Combine(temporaryFolder, "AutoActions.Updater.exe")))
                filesToCopy.Remove(Path.Combine(temporaryFolder, "AutoActions.Updater.exe"));
            if (filesToCopy.Contains(Path.Combine(temporaryFolder, "AutoActions.Updater.pdb")))
            filesToCopy.Remove(Path.Combine(temporaryFolder, "AutoActions.Updater.pdb"));
            foreach (string file in filesToCopy)
            {
                string targetFileName = Path.Combine(targetFolder, Path.GetFileName(file));
                if (File.Exists(targetFileName))
                    File.Delete(targetFileName);
                File.Move(file, targetFileName);
            }
            foreach (string file in updateData.FilesToDelete)
            {
                string targetFileName = Path.Combine(targetFolder, Path.GetFileName(file));
                if (File.Exists(targetFileName))
                    File.Delete(targetFileName);
            }
        }

        public static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        private static List<string> GetAllFiles(string directory)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(directory));
            foreach (var subDirectory in Directory.GetDirectories(directory))
                files.AddRange(GetAllFiles(subDirectory));
            return files;
        }
    }
}
