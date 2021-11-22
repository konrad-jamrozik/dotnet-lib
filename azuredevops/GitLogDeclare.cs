using Wikitools.Lib.Git;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps
{
    public class GitLogDeclare
    {
        public GitLog GitLog(IOperatingSystem os, Dir gitRepoDir, string gitExecutablePath)
        {
            var repo = new GitRepository(
                new GitBashShell(os, gitExecutablePath),
                gitRepoDir
            );
            var gitLog = new GitLog(repo);
            return gitLog;
        }
    }
}