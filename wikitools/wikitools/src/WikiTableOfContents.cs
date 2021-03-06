using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Data;
using Wikitools.Lib.Markdown;

namespace Wikitools
{
    public record WikiTableOfContents : MarkdownDocument
    {
        public WikiTableOfContents(Task<TreeData<WikiTocEntry>> data) : base(GetContent(data)) { }

        private static async Task<object[]> GetContent(Task<TreeData<WikiTocEntry>> dataTask)
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
            // - this report knows how to print out wikiTocEntry

            var preorderData = (await dataTask).AsPreorderEnumerable();

            var lines = preorderData.Select(entry => new string(' ', entry.depth * 2) + entry.value.Path);
            return lines.Cast<object>().ToArray();
        }
    }
}