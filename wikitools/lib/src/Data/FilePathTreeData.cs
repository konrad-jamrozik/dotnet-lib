using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wikitools.Lib.Data
{
    public record FilePathTreeData(IEnumerable<string> FilePaths) : TreeDataFromPaths<string, string>(
        FilePaths, SplitPath)
    {
        // kj2 need to think about better home for this. (I)Filesystem?
        public static IEnumerable<string> SplitPath(string path) => path.Split(Path.DirectorySeparatorChar);
    }
}