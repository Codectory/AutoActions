using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodectoryCore.Logging;
using Octokit;

namespace HDRProfile.Info.Github
{
    public static class GitHubIntegration
    {
        private static GitHubClient _client = new GitHubClient(new ProductHeaderValue("HDRProfile"));

        private static bool Initialized = false;
        private static void InitializeClient()
        {
            if (Initialized)
                return;
            Tools.Logs.Add($"Connecting to GitHub...", false);
            _client = new GitHubClient(new ProductHeaderValue("HDRProfile"));
            _client.SetRequestTimeout(new TimeSpan(0, 0, 10));
            Initialized = true;
        }
        public static GitHubData GetGitHubData()
        {
            InitializeClient();
            Tools.Logs.Add($"Requesting releases...", false);
            IReadOnlyList<Release> releases = _client.Repository.Release.GetAll("Codectory", "HDR-Profile").Result;

            Version latestGitHubVersion = new Version(releases[0].TagName);
            DateTime latestReleaseDate = releases[0].PublishedAt.HasValue ? releases[0].PublishedAt.Value.DateTime : DateTime.MinValue;
            Tools.Logs.Add($"Releases found: {releases.Count} Latest version: {latestGitHubVersion}", false);

            string sourceForgeAddition = "\n\n"+ @"[![Download HDR Profile]";
            string sourceForgeAddition2 = "\n" + @"[![Download HDR Profile]";
            string sourceForgeAddition3 = "\n" + @"[![Download HDR Profile]";

            string changelog = string.Empty;
            Tools.Logs.Add($"Building changelog...", false);

            foreach (Release release in releases)
            {
                if (!string.IsNullOrEmpty(changelog))
                    changelog += "\r\n\r\n\r\n\r\n";
                string releaseChangelog = release.Body;
                if (releaseChangelog.Contains(sourceForgeAddition))
                    releaseChangelog = releaseChangelog.Substring(0, releaseChangelog.IndexOf(sourceForgeAddition));
                if (releaseChangelog.Contains(sourceForgeAddition2))
                    releaseChangelog = releaseChangelog.Substring(0, releaseChangelog.IndexOf(sourceForgeAddition2));
                if (releaseChangelog.Contains(sourceForgeAddition3))
                    releaseChangelog = releaseChangelog.Substring(0, releaseChangelog.IndexOf(sourceForgeAddition3));

                changelog += $"[{release.TagName}]\r\n\r\n{releaseChangelog}";
            }
            Tools.Logs.Add($"Creating GitHubData...", false);
            return new GitHubData(changelog, latestGitHubVersion, latestReleaseDate, $@"https://github.com/Codectory/HDR-Profile/releases/tag/{latestGitHubVersion}");
        }
    }
}
