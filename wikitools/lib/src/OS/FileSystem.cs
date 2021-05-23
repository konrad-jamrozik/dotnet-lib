using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Json;

namespace Wikitools.Lib.OS
{
    public class FileSystem : IFileSystem
    {
        public static Dir? Parent(IFileSystem fs, string path)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(path), path);
            var parentDirInfo = new DirectoryInfo(path).Parent;
            return parentDirInfo != null ? new Dir(fs, parentDirInfo.FullName) : null;
        }

        public static IEnumerable<string> SplitPath(string path) => path.Split(Path.DirectorySeparatorChar);

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

        /// <remarks>
        /// Related issue:
        /// https://github.com/dotnet/docs/issues/24251
        /// </remarks>
        public JsonElement ReadAllJson(string path) => ReadAllBytes(path).FromJsonTo<JsonElement>();

        public T ReadAllJsonTo<T>(string path) => ReadAllBytes(path).FromJsonTo<T>();

        public Task<FilePathTrie> FileTree(string path)
        {
            var fileTree = new FileTree(this, path);
            return fileTree.FilePathTrie();
        }

        public Dir? Parent(string path) => Parent(this, path);

    }
}
