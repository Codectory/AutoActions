using AutoActions.Info.Github;
using System;

namespace AutoActions
{
    public class CheckUpdateResult
    {

        public bool UpdateAvailable { get; private set; } = false;
        public GitHubData GitHubData { get; private set; } = null;

        public CheckUpdateResult(bool updateAvailable, GitHubData gitHubData = null)
        {
            UpdateAvailable = updateAvailable;
            if (updateAvailable && gitHubData == null)
                throw new ArgumentNullException(nameof(gitHubData));
            GitHubData = gitHubData;
        }
    }
}