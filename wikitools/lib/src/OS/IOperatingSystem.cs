namespace Wikitools.Lib.OS
{
    public interface IOperatingSystem
    {
        IProcess Process(string executableFilePath, string workingDirPath, params string[] arguments);
    }
}