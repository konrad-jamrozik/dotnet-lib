using System.Collections.Generic;

namespace Wikitools.Lib.Git
{
    public interface IProcessSimulationSpec
    {
        List<string> StdOutLines { get; }
        bool Matches(string executableFilePath, string workingDirPath, string[] arguments);
    }
}