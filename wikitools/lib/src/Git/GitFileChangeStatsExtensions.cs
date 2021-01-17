using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Git
{
    // kja get rid of all extension classes.
    public static class GitFileChangeStatsExtensions
    {
        public static List<GitFileChangeStats> SumByFilePath(this List<GitFileChangeStats> changesStats)
        {
            var statsByFilePath = changesStats.GroupBy(log => log.FilePath);
            var statsSumByFile = statsByFilePath.Select(fileStats => new GitFileChangeStats(
                fileStats.Key,
                fileStats.Sum(log => log.Insertions),
                fileStats.Sum(log => log.Deletions)));
            return statsSumByFile.ToList();
        }

        public static GitFileChangeStats ToGitFileChangeStats(this string gitLogStdOutLine)
        {
            var split      = gitLogStdOutLine.Split('\t');
            var insertions = int.Parse(split[0].Replace("-", "0"));
            var deletions  = int.Parse(split[1].Replace("-", "0"));
            var filePath   = split[2];
            return new GitFileChangeStats(filePath, insertions, deletions);
        }
    }
}