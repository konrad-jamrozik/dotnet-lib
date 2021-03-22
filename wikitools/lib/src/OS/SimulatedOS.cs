using System.Linq;

namespace Wikitools.Lib.OS
{
    public record SimulatedOS(params IProcessSimulationSpec[] ProcessesSimulationsSpecs) : IOperatingSystem
    {
        public IProcess Process(string executableFilePath, string workingDirPath, params string[] arguments) =>
            new SimulatedProcess(ProcessesSimulationsSpecs.Single(
                spec => spec.Matches(executableFilePath, workingDirPath, arguments)).StdOutLines);

        public IOSEnvironment Environment { get; } = new OSEnvironment(); // kj2 this OS environment should be simulated
    }
}