using System.Linq;
using Wikitools.Lib.Git;

namespace Wikitools.Lib.OS
{
    public class SimulatedOS : IOperatingSystem
    {
        private readonly IProcessSimulationSpec[] _processesSimulationsSpecs;

        public SimulatedOS(params IProcessSimulationSpec[] processesSimulationsSpecs)
        {
            _processesSimulationsSpecs = processesSimulationsSpecs;
        }

        public IProcess Process(string executableFilePath, string workingDirPath, params string[] arguments)
        {
            var processSimulationSpec = _processesSimulationsSpecs.Single(spec => spec.Matches(executableFilePath, workingDirPath, arguments));

            return new SimulatedProcess(processSimulationSpec.StdOutLines);
        }

        public IFileSystem FileSystem { get; } = new FileSystem();
    }
}