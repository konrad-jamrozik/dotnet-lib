using System;
using System.Linq;

namespace Wikitools.Lib.OS;

public record SimulatedOS(params IProcessSimulationSpec[] ProcessesSimulationsSpecs) : IOperatingSystem
{
    public IProcess Process(string executableFilePath, Dir workingDir, params string[] arguments) =>
        new SimulatedProcess(SingleMatchingSpec(executableFilePath, workingDir, arguments).StdOutLines);

    private IProcessSimulationSpec SingleMatchingSpec(string executableFilePath, Dir workingDir, string[] arguments)
    {
        try
        {
            return ProcessesSimulationsSpecs.Single(
                spec => spec.Matches(executableFilePath, workingDir.Path, arguments));
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException(
                ("Did not find a single matching IProcessSimulationSpec.\\n"
                 + $"arguments: \\n{string.Join(", ", arguments)}\\n"
                 + "specs:\\n"
                 + $"{string.Join(System.Environment.NewLine, ProcessesSimulationsSpecs.Select(spec => spec.ToString()))}")
                .Replace("\\n",System.Environment.NewLine),
                e);
        }
    }
}