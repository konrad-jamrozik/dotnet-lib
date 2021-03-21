using System;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Data;

namespace Wikitools.Lib.OS
{
    public class SimulatedFileSystem : IFileSystem
    {
        private int _dirIndex;

        public SimulatedFileSystem(DirectoryInfo currentDirectoryInfo)
        {
            CurrentDirectoryInfo = currentDirectoryInfo;
        }

        public bool DirectoryExists(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task WriteAllTextAsync(string path, string contents)
        {
            throw new System.NotImplementedException();
        }

        public Dir CreateDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public string JoinPath(string? path1, string? path2)
        {
            throw new System.NotImplementedException();
        }

        public bool FileExists(string? path)
        {
            throw new System.NotImplementedException();
        }

        public DirectoryInfo CurrentDirectoryInfo { get; }
        public string CombinePath(string path1, string path2)
        {
            throw new System.NotImplementedException();
        }

        public string ReadAllText(string path)
        {
            throw new System.NotImplementedException();
        }

        public byte[] ReadAllBytes(string path)
        {
            throw new System.NotImplementedException();
        }

        public Task<FilePathTrie> FileTree(string path)
        {
            throw new System.NotImplementedException();
        }

        public Dir NextSimulatedDir() => new(this, "S:/simulatedDir" + _dirIndex++);
    }
}