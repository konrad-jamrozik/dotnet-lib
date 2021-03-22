using System;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Data;

namespace Wikitools.Lib.OS
{
    public class SimulatedFileSystem : IFileSystem
    {
        private int _dirIndex;

        public Dir CurrentDir => new(this, "S:/simulatedCurrentDir");

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public Task WriteAllTextAsync(string path, string contents)
        {
            throw new NotImplementedException();
        }

        public Dir CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public string JoinPath(string? path1, string? path2)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string? path)
        {
            throw new NotImplementedException();
        }


        public string CombinePath(string path1, string path2)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadAllBytes(string path)
        {
            throw new NotImplementedException();
        }

        public Task<FilePathTrie> FileTree(string path)
        {
            throw new NotImplementedException();
        }

        public Dir Parent(string path)
        {
            throw new NotImplementedException();
        }

        public Dir NextSimulatedDir() => new(this, CurrentDir.JoinPath($"simulatedDir{_dirIndex++}"));
    }
}