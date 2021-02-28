using System.Threading.Tasks;
using Wikitools.Lib.Markdown;

namespace Wikitools
{
    public record WikiTableOfContents : MarkdownDocument
    {
        public WikiTableOfContents(Task<object[]> data) : base(GetContent(data)) { }

        private static Task<object[]> GetContent(Task<object[]> data)
        {
            // kja current work
            // Build tree of relative paths, indented by depth
            // Input:
            // DataTree<WikiTocEntry>
            // - enumerable by lexicographically sorted DFS, i.e. in the order it should
            //   be printed out.
            // WikiTocEntry: (int depth, WikiPagePath path, WikiPageStats stats)
            // - depth will be used to compute indent
            // - WikiPagePath can be enumerated for each components
            //   - this report knows how to convert path to hyperlink
            //     - hyperlink conversion probably should be abstracted to be generic
            // - stats will be used to compute if icons should show: new, active, stale
            //  - thresholds for icons passed separately as param, coming from config
            // - this report knows how to print out wikiTocEntry
            return data;
        }
    }
}