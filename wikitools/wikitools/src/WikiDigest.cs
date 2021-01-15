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
        PageViewsStatsReport PageViews) : IWritableToText
    {
        // kja pass as input: DayOfWeek, authors top limit, paths in wiki to ignore (both for files report and page views)
        // kja make the digest check the day, and if it is time for a new one, do the following:
        // - pull the new digest data from git and ado wiki api
        // - save the new digest data to a json file
        // - create a new md digest file in local repo clone
        // - inform on stdout its time to manually review, commit and push the change
        public async Task WriteAsync(TextWriter textWriter)
        {
            var topAuthors = Authors with { Rows = Authors.Rows.M(rows => rows.Take(5).ToList()) };
            var topFiles = Files with { Rows = Files.Rows.M(rows => rows.Take(5).ToList()) };
            await new MarkdownTable(topAuthors).WriteAsync(textWriter);
            await new MarkdownTable(topFiles).WriteAsync(textWriter);
            await new MarkdownTable(PageViews).WriteAsync(textWriter);
        }
    }
}