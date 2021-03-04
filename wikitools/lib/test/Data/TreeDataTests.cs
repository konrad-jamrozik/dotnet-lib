using Wikitools.Lib.Data;
using Xunit;

namespace Wikitools.Lib.Tests.Data
{
    public class TreeDataTests
    {
        [Fact]
        public void TestTreeData()
        {
            var treeData = new TreeData<string>(new[] { "foo/bar1", "foo/bar2" });
            // kja curr work. See the todo in Wikitools.Lib.Data.TreeData
        }
    }
}