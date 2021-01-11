using System.IO;

namespace Wikitools.Lib.OS
{
    public class FileSystem : IFileSystem
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);
    }
}
