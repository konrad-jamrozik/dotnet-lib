using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.Lib.OS;

public class SimulatedProcess : IProcess
{
    private readonly List<string> _stdOutLines;

    public SimulatedProcess(List<string> stdOutLines) 
        => _stdOutLines = stdOutLines;

    public Task<List<string>> GetStdOutLines() => Task.FromResult(_stdOutLines);
}