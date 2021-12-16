namespace Wikitools.Lib.OS;

public class WindowsOS : IOperatingSystem
{
    public IProcess Process(string executableFilePath, Dir workingDir, params string[] arguments) 
        => new Process(executableFilePath, workingDir, arguments);
}