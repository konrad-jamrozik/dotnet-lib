using System.Linq;

namespace Wikitools.Lib.OS
{
    public record SimulatedOS(params IProcessSimulationSpec[] ProcessesSimulationsSpecs) : IOperatingSystem
    {
        public IProcess Process(string executableFilePath, Dir workingDir, params string[] arguments) =>
            new SimulatedProcess(ProcessesSimulationsSpecs.Single(
                spec => spec.Matches(executableFilePath, workingDir.Path, arguments)).StdOutLines);
    }
}