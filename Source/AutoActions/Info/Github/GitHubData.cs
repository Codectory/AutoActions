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

        public GitHubData(string changeLog, Version currentVersion, DateTime lastReleaseDate, string downloadLink = "")
        {
            ChangeLog = changeLog ?? throw new ArgumentNullException(nameof(changeLog));
            CurrentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            LastReleaseDate = lastReleaseDate;
            DownloadLink = downloadLink;
        }
    }
}
