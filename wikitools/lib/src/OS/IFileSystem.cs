using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Data;

namespace Wikitools.Lib.OS
{
    public interface IFileSystem
    {
        bool DirectoryExists(string path);
        public Task WriteAllTextAsync(string path, string contents);
        DirectoryInfo CreateDirectory(string path);
        string JoinPath(string? path1, string? path2);
        bool FileExists(string? path);
        DirectoryInfo CurrentDirectoryInfo { get; }
        string CombinePath(string path1, string path2);
        string ReadAllText(string path);
        byte[] ReadAllBytes(string path);
        Task<FilePathTrie> FileTree(string path);
    }
}