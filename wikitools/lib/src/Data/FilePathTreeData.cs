using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wikitools.Lib.Data
{
    public record FilePathTreeData(IEnumerable<string> Rows) : TreeData<string, string>(
        Rows,
        path => path.Split(Path.DirectorySeparatorChar).ToArray())
    {
    }
}