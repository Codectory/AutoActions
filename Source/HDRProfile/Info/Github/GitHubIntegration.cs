using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodectoryCore.Logging;
using Octokit;

namespace AutoHDR.Info.Github
{
    public static class GitHubIntegration
    {
        private static GitHubClient _client = new GitHubClient(new ProductHeaderValue("AutoHDR"));

        private static bool Initialized = false;
        private static void InitializeClient()
        {
            if (Initialized)
                return;
            Globals.Logs.Add($"Connecting to GitHub...", false);
            _client = new GitHubClient(new ProductHeaderValue("AutoHDR"));
            _client.SetRequestTimeout(new TimeSpan(0, 0, 10));
            Initialized = true;
        }
        public static GitHubData GetGitHubData()
        {
            InitializeClient();
            Globals.Logs.Add($"Requesting releases...", false);
            IReadOnlyList<Release> releases = _client.Repository.Release.GetAll("Codectory", "AutoHDR").Result;

            Version latestGitHubVersion = new Version(releases[0].TagName);
            DateTime latestReleaseDate = releases[0].PublishedAt.HasValue ? releases[0].PublishedAt.Value.DateTime : DateTime.MinValue;
            Globals.Logs.Add($"Releases found: {releases.Count} Latest version: {latestGitHubVersion}", false);

            List<string> sourceForgeAdditions = new List<string>()
            {
                "\n\n"+ @"[![Download HDR Profile]",
                "\n" + @"[![Download HDR Profile]",
                "\n" + @"[![Download HDR Profile]",
                "\n\n" + @"[![Download AutoHDR]",
                "\n" + @"[![Download AutoHDR]",
                "\n" + @"[![Download AutoHDR]"
            };

            string changelog = string.Empty;
            Globals.Logs.Add($"Building changelog...", false);

            foreach (Release release in releases)
            {
                if (!string.IsNullOrEmpty(changelog))
                    changelog += "\r\n\r\n\r\n\r\n";
                string releaseChangelog = release.Body;
                foreach (string sourceForgeAddition in sourceForgeAdditions)
                {
                    if (releaseChangelog.Contains(sourceForgeAddition))
                        releaseChangelog = releaseChangelog.Substring(0, releaseChangelog.IndexOf(sourceForgeAddition));

                }

                changelog += $"[{release.TagName}]\r\n\r\n{releaseChangelog}";
            }
            Globals.Logs.Add($"Creating GitHubData...", false);
            return new GitHubData(changelog, latestGitHubVersion, latestReleaseDate, $@"https://github.com/Codectory/HDR-Profile/releases/tag/{latestGitHubVersion}");
        }
    }
}
