using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.OS
{
    public class FileSystem : IFileSystem
    {
        public Dir CurrentDir => new (this, Directory.GetCurrentDirectory());

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public Task WriteAllTextAsync(string path, string contents) 
            => File.WriteAllTextAsync(path, contents);

        public Dir CreateDirectory(string path)
        {
            var directoryInfo = Directory.CreateDirectory(path);
            return new Dir(this, directoryInfo.FullName);
        }

        public string JoinPath(string? path1, string? path2) => Path.Join(path1, path2);

        public bool FileExists(string path) => File.Exists(path);

        public string CombinePath(string path1, string path2) => Path.Combine(path1, path2);

        public string ReadAllText(string path) => File.ReadAllText(path);

        public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
        
        public Task<FilePathTrie> FileTree(string path)
        {
            var fileTree = new FileTree(this, path);
            return fileTree.FilePathTrie();
        }

        public Dir? Parent(string path) => Parent(this, path);

        public static Dir? Parent(IFileSystem fs, string path)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(path), path);
            var parentDirInfo = new DirectoryInfo(path).Parent;
            return parentDirInfo != null ? new Dir(fs, parentDirInfo.FullName) : null;
        }

        public static IEnumerable<string> SplitPath(string path) => path.Split(Path.DirectorySeparatorChar);
    }
}
