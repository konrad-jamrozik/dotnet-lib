using System.IO;
using System.Threading.Tasks;

namespace Wikitools.Lib.OS
{
    public record Dir(IFileSystem FileSystem, string Path)
    {
        public bool Exists() => FileSystem.DirectoryExists(Path);

        public Dir CreateDirIfNotExists() => !Exists() ? CreateDir() : this;

        public Dir CreateDir() => FileSystem.CreateDirectory(Path);

        public bool FileExists(string fileName) => FileSystem.FileExists(JoinPath(fileName));

        public string JoinPath(string fileName) => FileSystem.JoinPath(Path, fileName);

        public string ReadAllText(string fileName) => FileSystem.ReadAllText(JoinPath(fileName));

        public string[] ReadAllLines(string fileName)
            => FileSystem.ReadAllLines(JoinPath(fileName));

        public Task WriteAllTextAsync(string fileName, string contents) =>
            FileSystem.WriteAllTextAsync(JoinPath(fileName), contents);

        public StreamWriter CreateText(string fileName)
            => FileSystem.CreateText(JoinPath(fileName));

        public Dir? Parent => FileSystem.Parent(Path);
    }
}