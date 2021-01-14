using System.IO;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public record WikiDigest(
        GitAuthorsStatsReport GitAuthorsReport,
        GitFilesStatsReport GitFilesReport,
        PageViewsStatsReport PageViewsReport) : IWritableToText
    {
        // kja pass as input: DayOfWeek, authors top limit, paths in wiki to ignore (both for files report and page views)
        // kja make the digest check the day, and if it is time for a new one, do the following:
        // - pull the new digest data from git and ado wiki api
        // - save the new digest data to a json file
        // - create a new md digest file in local repo clone
        // - inform on stdout its time to manually review, commit and push the change
        public async Task WriteAsync(TextWriter textWriter)
        {
            // kja try here to limit the rows output by deconstructing into tuple, doing .Take() on rows, and rapping in new MarkdownTable
            // I can get deconstructor for free if I will ensure this is a positional record: https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9
            await new MarkdownTable(GitAuthorsReport).WriteAsync(textWriter);
            await new MarkdownTable(GitFilesReport).WriteAsync(textWriter);
            await new MarkdownTable(PageViewsReport).WriteAsync(textWriter);
        }
    }
}