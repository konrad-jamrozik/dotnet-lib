using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Wikitools.Lib.Json;

namespace Wikitools.Lib.OS;

/// <summary>
/// This class aims to provides partial in-memory implementation
/// that replicates behavior of Wikitools.Lib.OS.FileSystem.
///
/// It is developed on an "as needed" basis, i.e. the current implementation
/// closely replicates the behavior of Wikitools.Lib.OS.FileSystem,
/// but only to the degree that makes all tests using this class pass,
/// operating under the same relevant assumptions as actual implementation.
///
/// For example, if the code exercised by given test calls
/// Wikitools.Lib.OS.FileSystem.CreateDirectory,
/// this will create the entire directory tree for given path that doesn't exist.
/// However, if the fact that all parent directories were created is not pertinent
/// for the test, or any other test, SimulatedFileSystem might end up simulating
/// creation of only the leaf directory.
///
/// This behavior doesn't exactly match the behavior of FileSystem, by design.
/// Only when there will be at least one test using SimulatedFileSystem that relies
/// on the fact that all parent directories are correctly created, this class
/// will implement this.
/// </summary>
public class SimulatedFileSystem : IFileSystem
{
    private int _dirIndex;

    public Dir CurrentDir => new(this, JoinPath("S:" + Path.DirectorySeparatorChar, "simulatedCurrentDir"));

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

    public Task WriteAllLinesAsync(string path, IEnumerable<string> lines)
    {
        throw new NotImplementedException();
    }

    public StreamWriter CreateText(string path)
    {
        throw new NotImplementedException();
    }

    public string[] ReadAllLines(string path)
    {
        throw new NotImplementedException();
    }

    public Dir CreateDirectory(string path)
    {
        _existingDirs.Add(path);
        return new Dir(this, path);
    }

    public string JoinPath(string? path1, string? path2) => Path.Join(path1, path2);

    public bool FileExists(string path) => _existingFiles.Contains(path);

    public string CombinePath(string path1, string path2)
    {
        throw new NotImplementedException();
    }

    public string ReadAllText(string path) =>
        !_existingFiles.Contains(path)
            ? throw new FileNotFoundException(path)
            : _fileContents[path];

    public JsonElement ReadAllJson(string path) => ReadAllBytes(path).FromJsonTo<JsonElement>();

    public T ReadAllJsonTo<T>(string path) => ReadAllBytes(path).FromJsonTo<T>();

    public byte[] ReadAllBytes(string path) => Encoding.UTF8.GetBytes(ReadAllText(path));

    public FileTree FileTree(string path)
    {
        throw new NotImplementedException();
    }

    public Dir? Parent(string path) => FileSystem.Parent(this, path);

    public Dir NextSimulatedDir() => new(this, CurrentDir.JoinPath($"simulatedDir{_dirIndex++}"));
}