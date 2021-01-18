﻿using System.Linq;
using Wikitools.Lib.Git;

namespace Wikitools.Lib.OS
{
    public record SimulatedOS(params IProcessSimulationSpec[] ProcessesSimulationsSpecs) : IOperatingSystem
    {
        public IProcess Process(string executableFilePath, string workingDirPath, params string[] arguments) =>
            new SimulatedProcess(ProcessesSimulationsSpecs.Single(
                spec => spec.Matches(executableFilePath, workingDirPath, arguments)).StdOutLines);

        public IFileSystem FileSystem { get; } = new FileSystem();
    }
}