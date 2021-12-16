using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git;

public record GitBashShell(IOperatingSystem OS, string GitExecutablePath) : IShell
{
    public Task<List<string>> GetStdOutLines(Dir workingDir, string[] arguments)
    {
        var executableFilePath = GitExecutablePath.Replace(@"\", @"\\");

        // Reference:
        // https://stackoverflow.com/questions/17302977/how-to-launch-git-bash-from-dos-command-line
        // https://superuser.com/questions/1104567/how-can-i-find-out-the-command-line-options-for-git-bash-exe
        string[] processArguments =
        {
            "--login", 
            "-c", 
            new QuotedString(string.Join(" ", arguments)).Value
        };
            
        IProcess process = OS.Process(executableFilePath, workingDir, processArguments);

        return process.GetStdOutLines();
    }
}