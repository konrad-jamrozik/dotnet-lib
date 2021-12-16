using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib;

public class GitLogDeclare
{
    public GitLog GitLog(
        ITimeline timeline,
        IOperatingSystem os,
        Dir gitRepoDir,
        string gitExecutablePath)
    {
        var repo = new GitRepository(
            new GitBashShell(os, gitExecutablePath),
            gitRepoDir
        );
        var gitLog = new GitLog(timeline, repo);
        return gitLog;
    }
}