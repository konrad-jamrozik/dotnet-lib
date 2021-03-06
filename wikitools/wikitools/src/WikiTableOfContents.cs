using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Data;
using Wikitools.Lib.Markdown;

namespace Wikitools
{
    public record WikiTableOfContents : MarkdownDocument
    {
        public WikiTableOfContents(Task<IEnumerable<WikiTocEntry>> data) : base(GetContent(data)) { }

        private static async Task<object[]> GetContent(Task<IEnumerable<WikiTocEntry>> dataTask)
        {
            // kja pseudocode for WikiTableOfContents
            // Build tree of relative paths, indented by depth
            // Input:
            // TreeData<WikiTocEntry>
            // - enumerable by lexicographically sorted DFS, i.e. in the order it should
            //   be printed out.
            // WikiTocEntry: (int depth, FilePath path, WikiPageStats stats)
            // - depth will be used to compute indent
            // - FilePath can be enumerated for each components
            //   - this report knows how to convert path to hyperlink
            //     - hyperlink conversion probably should be abstracted to be generic: in MarkdownDocument
            // - stats will be used to compute if icons should show: new, active, stale
            //  - thresholds for icons passed separately as param, coming from config
            // - this report knows how to print out wikiTocEntry.
            // See the to-do in Wikitools.Program.GetWikiPagesData for more.

            // Example output:
            // [/foo](foo) 10 views
            //     [/bar](foo/bar) 30 views

            var preorderData = await dataTask;

            // kja need to add here ability to "erase" path prefixes of the parent above.
            // That is, instead of displaying:
            // Foo
            // Foo/Bar1
            // Foo/Bar2
            // Display:
            // Foo
            //   Bar1
            //   Bar2
            //
            // To do this elegantly, I need to grok how to convert FilePathTreeData
            // to TreeData<WikiTocEntry>
            // Note that now WikiTocEntry has Path and no Depth.
            // Depth would be redundant with abstract TreeData depth,
            // Observe also that TreeNode has (depth, TValue), so that
            // would be (depth, WikiTocEntry) which is (depth, (Path, Stats))
            // but we don't really want full Path, only the leaf.
            // It also doesn't really make sense, as the TValue should be TSegment,
            // not full TPath.
            // kja also display relevant icon, based on stats.
            var lines = preorderData.Select(entry => entry.Path);
            return lines.Cast<object>().ToArray();
        }
    }
}