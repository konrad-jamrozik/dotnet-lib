using System.Collections.Generic;

namespace Wikitools.Lib.OS;

public interface IProcessSimulationSpec
{
    List<string> StdOutLines { get; }
    bool Matches(string executableFilePath, string workingDirPath, string[] arguments);
}