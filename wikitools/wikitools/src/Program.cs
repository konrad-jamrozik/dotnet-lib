using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools;
using Wikitools.AzureDevOps;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using static Wikitools.Declare;

var cfg      = WikitoolsConfig.From("wikitools_config.json");
var timeline = new Timeline();
var os       = new WindowsOS();
var adoApi   = new AdoApi();
//
var gitLog           = GitLog(os, cfg.GitRepoClonePath, cfg.GitExecutablePath, cfg.GitLogDays);
var wiki             = Wiki(adoApi, cfg.AdoWikiUri, cfg.AdoPatEnvVar, cfg.AdoWikiPageViewsForDays);
//
var gitAuthorsReport = new GitAuthorsStatsReport(timeline, gitLog, cfg.GitLogDays);
var gitFilesReport   = new GitFilesStatsReport(timeline, gitLog, cfg.GitLogDays);
var pageViewsReport  = new PagesViewsStatsReport(timeline, wiki, cfg.AdoWikiPageViewsForDays);
var wikiDigest       = new WikiDigest(gitAuthorsReport, gitFilesReport, pageViewsReport);

var desc               = $"Git contributions since last {cfg.GitLogDays} days as of {timeline.UtcNow}";
var headerRow          = new List<string> { "Place", "Author", "Insertions", "Deletions" };
var authorChangesStats = GetAuthorsChangesStatsRows(gitLog.GetAuthorChangesStats());

var outputData = new List<IFormattableAsMarkdown>
{
    new TextLines2(desc),
    new TabularData2(headerRow, authorChangesStats)
};

async Task<List<List<object>>> GetAuthorsChangesStatsRows(Task<List<GitAuthorChangeStats>> authorChangesStats)
{

    GitAuthorChangeStats[] authorsStatsOrdered = (await gitLog.GetAuthorChangesStats()).SumByAuthor()
        .OrderByDescending(authorStats => authorStats.Insertions + authorStats.Deletions)
        .Where(stats => !stats.Author.Contains("Konrad J"))
        .ToArray()[..5];

    return authorsStatsOrdered.Select((stats, i) =>
            new List<object> { i, stats.Author, stats.FilesChanged, stats.Insertions, stats.Deletions })
        .ToList();
}

/*

reportData = 
{
Description = "Wiki insertions by month, from MM-DD-YYYY to MM-DD-YYYY, both inclusive."
TabularData = new TabularData(
  HeaderRow = Month, Insertions, Monthly Change
  Rows = 
    List<List<GitFileChangeStats>> monthly change lists = foreach <month start and end dates>: GitLog.GetFilesChanges(start month, end month)
    monthly insertions sums = foreach monthly change list: sum insertions
    aggregate: monthly insertion sums: add MoM change. 
    transpose aggregate: instead of two lists (List(sums), List(MoM)), make it List(Month(sum), Month(MoM)) (see https://morelinq.github.io/3.0/ref/api/html/M_MoreLinq_MoreEnumerable_Transpose__1.htm)
)
}

reportData.WriteOutAsMarkdown(Console.Out); // this will "uplift" the data into markdown, including converting all TabularData-s into MarkdownTable-s

 */
//string serialized = obj.ToIndentedJsonString();

//Console.Out.WriteLine(serialized);

// kja return table of monthly wiki contributions, sum of insertions per month, MoM % change

// kja core functionality, temp commented out while experimenting
//await wikiDigest.WriteAsync(Console.Out);


// kja here there should be a step lazily that composes the data structure, and then writes it out
// var obj = new
// {
//     channel = new
//     {
//         title = "James Newton-King",
//         link = "http://james.newtonking.com",
//         description = "James Newton-King's blog.",
//         item =
//             from p in new List<dynamic>
//             {
//                 new
//                 {
//                     Title = "fooTitle", Description = "fooDescription", Link = "fooLink",
//                     Categories = new List<int> { 1, 2 }
//                 }
//             }
//             orderby p.Title
//             select new
//             {
//                 title = p.Title,
//                 description = p.Description,
//                 link = p.Link,
//                 category = p.Categories
//             }
//     }
// };