using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wikitools.Lib.OS;

public record FileTree(IFileSystem FileSystem, string Path)
{
    // kj2-toc unused, as now I use instead AdoWikiPagesPaths
    // kj2-toc consider making FileTree implement TreeData<string> instead
    // This will possibly require making the record abstract, and also
    // the problem of task / laziness needs to be addressed
    // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree
    public FilePathTrie FilePathTrie()
    {
        var paths = Paths;
        return new FilePathTrie(paths.ToArray());
    }

    public IEnumerable<string> Paths
    {
        get
        {
            // kj2-toc FileTree.Paths / make this method Lazy
            // kj2-toc implement properly walking the tree: decoupled from IFileSystem
            var directoryInfo = new DirectoryInfo(Path);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
            var paths = fileInfos.Select(fi => System.IO.Path.GetRelativePath(Path, fi.FullName));
            return new SortedSet<string>(paths);
        }
    }
}