using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.Lib.Git
{
    public record GitRepository(GitBashShell GitBashShell, string WorkingDirPath)
    {
        public Task<List<string>> GetStdOutLines(string gitCmdLine) 
            => GitBashShell.GetStdOutLines(WorkingDirPath, new[] {gitCmdLine});
    }
}
