using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps
{
    public class GitLogDeclare
    {
        public GitLog GitLog(
            ITimeline timeline,
            IOperatingSystem os,
            Dir gitRepoDir,
            string gitExecutablePath)
        {
            // kja inline
            return new GitLogDeclare2().GitLog(timeline, os, gitRepoDir, gitExecutablePath);
        }
    }
}