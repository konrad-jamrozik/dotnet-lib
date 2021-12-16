using System.Collections.Generic;
using Wikitools.Lib.Data;

namespace Wikitools.Lib.OS;

public record FilePathTrie(IEnumerable<string> FilePaths) : TrieFromPaths(
    FilePaths, FileSystem.SplitPath)
{ }