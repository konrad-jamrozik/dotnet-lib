using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Data;

namespace Wikitools.Lib.OS
{
    public interface IFileSystem
    {
        bool DirectoryExists(string path);
        public Task WriteAllTextAsync(string path, string contents);
        Dir CreateDirectory(string path);
        string JoinPath(string? path1, string? path2);
        bool FileExists(string? path);
        DirectoryInfo CurrentDirectoryInfo { get; } // kj2 IFileSystem should not depend on DirectoryInfo
        string CombinePath(string path1, string path2);
        string ReadAllText(string path);
        byte[] ReadAllBytes(string path);
        Task<FilePathTrie> FileTree(string path);
    }
}