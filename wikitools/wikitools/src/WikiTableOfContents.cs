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
            return data;
        }
    }
}