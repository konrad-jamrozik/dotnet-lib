using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wikitools.Lib.Data
{
    public record FilePathTreeData(IEnumerable<string> Rows) : TreeDataFromPaths<string, string>(
        Rows,
        path => path.Split(Path.DirectorySeparatorChar).ToArray())
    {
    }
}