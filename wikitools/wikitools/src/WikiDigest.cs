using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record WikiDigest(
        GitAuthorsStatsReport Authors,
        GitFilesStatsReport Files,
        PagesViewsStatsReport PagesViews) : IWritableToText
    {
        // kja pass as input: DayOfWeek, authors top limit, paths in wiki to ignore (both for files report and page views)
        // kja make the digest check the day, and if it is time for a new one, do the following:
        // - pull the new digest data from git and ado wiki api
        // - save the new digest data to a json file
        // - create a new md digest file in local repo clone
        // - inform on stdout its time to manually review, commit and push the change
        public async Task WriteAsync(TextWriter textWriter)
        {
            // kja several problems here:
            // - the filtering out will screw up place indexing (column 0). Probably the indexes and row ordering should not be part of the report itself.
            //    - alternatively, we could say this is exactly what report is responsible for: all such filters. And make it part of it, and digest only for composing.
            //    - I like the idea of moving this to report responsibilities.
            //    - Before doing that, probably first deduplicate the logic in the 2 report classes, make them differ only by the filter lambda.
            // - these operations could be declared lazily, instead of in this eager block.
            // - would be nice to have strong typing on the rows.
            //   - The 2 report classes are already duplicated, also duplicating the test logic.
            //   - So strong typing would have to be achieved by a generic typing over the type of the row
            // - Overall the reports would vary by the row type T and the row filtering lambda F.
            var topAuthors = Authors with
            {
                Rows = Authors.Rows.M(rows => rows
                    .Where(row => !((string) row[1]).Contains("Konrad J"))
                    .Take(5)
                    .ToList()
                )
            };
            var topFiles = Files with
            {
                Rows = Files.Rows.M(rows => rows
                    .Where(row => !((string) row[1]).Contains("/Meta"))
                    .Take(5)
                    .ToList()
                )
            };
            await new MarkdownTable(topAuthors).WriteAsync(textWriter);
            await new MarkdownTable(topFiles).WriteAsync(textWriter);
            await new MarkdownTable(PagesViews).WriteAsync(textWriter);
        }
    }
}