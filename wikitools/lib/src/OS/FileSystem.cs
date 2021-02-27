using System.IO;
using System.Threading.Tasks;

namespace Wikitools.Lib.OS
{
    public class FileSystem : IFileSystem
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);

        public Task WriteAllTextAsync(string path, string contents) 
            => File.WriteAllTextAsync(path, contents);

        public DirectoryInfo CreateDirectory(string path) => Directory.CreateDirectory(path);

        public string JoinPath(string? path1, string? path2) => Path.Join(path1, path2);

        public bool FileExists(string? path) => File.Exists(path);

        public DirectoryInfo CurrentDirectoryInfo => new(Directory.GetCurrentDirectory());
        
        public string CombinePath(string path1, string path2) => Path.Combine(path1, path2);

        public string ReadAllText(string path) => File.ReadAllText(path);

        public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
    }
}
