namespace Wikitools.Lib.OS
{
    public class WindowsOS : IOperatingSystem
    {
        public IProcess Process(string executableFilePath, string workingDirPath, params string[] arguments)
        {
            var workingDir = new Dir(FileSystem, workingDirPath);
            return new Process(executableFilePath, workingDir, arguments);
        }

        public IFileSystem FileSystem { get; } = new FileSystem();

        public IOSEnvironment Environment { get; } = new OSEnvironment();
    }
}