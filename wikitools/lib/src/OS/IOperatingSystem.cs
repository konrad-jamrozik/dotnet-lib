namespace Wikitools.Lib.OS
{
    // kj2 move FileSystem and Environment out of this abstraction
    public interface IOperatingSystem
    {
        IProcess Process(string executableFilePath, string workingDirPath, params string[] arguments);
        IOSEnvironment Environment { get; }
    }
}