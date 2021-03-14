namespace Wikitools.Lib.OS
{
    // KJ2 move FileSystem and Environment out of this abstraction
    public interface IOperatingSystem
    {
        IProcess Process(string executableFilePath, string workingDirPath, params string[] arguments);
        IFileSystem FileSystem { get; }
        IOSEnvironment Environment { get; }
    }
}