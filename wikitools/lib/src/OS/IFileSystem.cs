using System.Threading.Tasks;

namespace Wikitools.Lib.OS
{
    public interface IFileSystem
    {
        Dir CurrentDir { get; }
        bool DirectoryExists(string path);
        public Task WriteAllTextAsync(string path, string contents);
        Dir CreateDirectory(string path);
        string JoinPath(string? path1, string? path2);
        bool FileExists(string path);
        string CombinePath(string path1, string path2);
        string ReadAllText(string path);
        byte[] ReadAllBytes(string path);
        Task<FilePathTrie> FileTree(string path);
        Dir? Parent(string path);
    }
}