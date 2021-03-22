namespace Wikitools.Lib.OS
{
    public class WindowsOS : IOperatingSystem
    {
        public IProcess Process(string executableFilePath, string workingDirPath, params string[] arguments)
        {
            var workingDir = new Dir(_fs, workingDirPath);
            return new Process(executableFilePath, workingDir, arguments);
        }

        // kj2 instead of this, the Process should take Dirs as params, not strings. These Dirs will have a handle to FS.
        private readonly IFileSystem _fs = new FileSystem();
    }
}