using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wikitools.Lib.OS;

public interface IFileSystem
{
    Dir CurrentDir { get; }
    bool DirectoryExists(string path);
    public Task WriteAllTextAsync(string path, string contents);
    public Task WriteAllLinesAsync(string path, IEnumerable<string> lines);
    public StreamWriter CreateText(string path);
    public string[] ReadAllLines(string path);
    Dir CreateDirectory(string path);
    string JoinPath(string? path1, string? path2);
    bool FileExists(string path);
    string CombinePath(string path1, string path2);
    string ReadAllText(string path);
    JsonElement ReadAllJson(string path);
    T ReadAllJsonTo<T>(string path);
    byte[] ReadAllBytes(string path);
    FileTree FileTree(string path);
    Dir? Parent(string path);
}