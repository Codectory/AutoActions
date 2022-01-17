using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions.Info.Github
{
    public class GitHubData
    {
        public string ChangeLog;
        public Version CurrentVersion;
        public DateTime LastReleaseDate;
        public string DownloadLink;
        public string DirectDownload64;
        public string DirectDownload86;

        public GitHubData(string changeLog, Version currentVersion, DateTime lastReleaseDate, string downloadLink, string downloadx64, string downloadx86)
        {
            ChangeLog = changeLog ?? throw new ArgumentNullException(nameof(changeLog));
            CurrentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            LastReleaseDate = lastReleaseDate;
            DownloadLink = downloadLink;
            DirectDownload64 = downloadx64;
            DirectDownload86 = downloadx86;

            string osVersion = Environment.Is64BitOperatingSystem ? "x64" : "x86";

        }
    }
}
