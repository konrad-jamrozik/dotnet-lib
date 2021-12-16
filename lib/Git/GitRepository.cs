using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Git;

public record GitRepository(GitBashShell GitBashShell, Dir WorkingDir)
{
    public Task<List<string>> GetStdOutLines(string gitCmdLine) 
        => GitBashShell.GetStdOutLines(WorkingDir, new[] {gitCmdLine});
}