using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.OS;

class CmdShell : IShell
{
    private readonly IOperatingSystem _os;

    public CmdShell(IOperatingSystem os) => _os = os;

    public Task<List<string>> GetStdOutLines(Dir workingDir, string[] arguments)
    {
        // Reference: 
        // https://ss64.com/nt/start.html
        // https://stackoverflow.com/questions/1469764/run-command-prompt-commands
        var executableFilePath = "cmd.exe";
        string[] processArguments =
        {
            "/C", 
            new QuotedString(string.Join(" ", arguments)).Value
        };

        IProcess process = _os.Process(executableFilePath, workingDir, processArguments);
        return process.GetStdOutLines();
    }
}