using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Git
{
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
            // kja to implement
            return new GitFileChangeStats("", 0, 0);
        }
    }
}