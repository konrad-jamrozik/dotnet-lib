using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Data;

namespace Wikitools.Lib.OS
{
    /// <summary>
    /// This class aims to provide in-memory implementation that replicates behavior
    /// of Wikitools.Lib.OS.FileSystem.
    ///
    /// It is developed on an "as needed" basis, i.e. the current implementation
    /// closely replicates the behavior of Wikitools.Lib.OS.FileSystem,
    /// but only to the degree that makes all tests using this class pass,
    /// operating under the same relevant assumptions as actual implementation.
    ///
    /// For example, if the code exercised by given test calls
    /// Wikitools.Lib.OS.FileSystem.CreateDirectory
    /// this will create the entire directory tree for given path that doesn't exist.
    /// However, if the fact that all parent directories were created is not pertinent
    /// for the test, or any other test, SimulatedFileSystem might end up simulating
    /// creation only of the leaf directory.
    ///
    /// This behavior doesn't exactly match the behavior of FileSystem, by design.
    /// Only when there will be at least one test using SimulatedFileSystem that relies
    /// on the fact that all parent directories are correctly created, this class
    /// will be guaranteed to implement this.
    /// </summary>
    public class SimulatedFileSystem : IFileSystem
    {
        private int _dirIndex;

        public Dir CurrentDir => new(this, "S:/simulatedCurrentDir");

        private readonly ISet<string> _existingDirs = new HashSet<string>();
        private readonly ISet<string> _existingFiles = new HashSet<string>();

        private readonly Dictionary<string, string> _fileContents = new();

        public bool DirectoryExists(string path) => _existingDirs.Contains(path);

        public Task WriteAllTextAsync(string path, string contents)
        {
            _existingFiles.Add(path);
            _fileContents[path] = contents;
            return Task.CompletedTask;
        }

        public Dir CreateDirectory(string path)
        {
            _existingDirs.Add(path);
            return new Dir(this, path);
        }

        public string JoinPath(string? path1, string? path2)
        {
            return path1 + "/" + path2;
        }

        public bool FileExists(string path) => _existingFiles.Contains(path);

        public string CombinePath(string path1, string path2)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path)
        {
            if (!_existingFiles.Contains(path))
                throw new FileNotFoundException(path);
            return _fileContents[path];
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