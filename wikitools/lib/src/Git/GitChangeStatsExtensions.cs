using System.Collections.Generic;
using System.Linq;

namespace Wikitools.Lib.Git
{
    public static class GitChangeStatsExtensions
    {
        public static List<GitAuthorChangeStats> SumByAuthor(this List<GitAuthorChangeStats> changesStats)
        {
            var statsByAuthor = changesStats.GroupBy(log => log.Author);
            var statsSumByAuthor = statsByAuthor.Select(authorStats => new GitAuthorChangeStats(
                authorStats.Key,
                authorStats.Sum(log => log.FilesChanged), 
                authorStats.Sum(log => log.Insertions),
                authorStats.Sum(log => log.Deletions)));
            return statsSumByAuthor.ToList();
        }

        public static List<string> FromGitLogStdOutLines(this List<GitAuthorChangeStats> changesStats) =>
            changesStats.SelectMany(stats =>
            {
                string authorLine = stats.Author;
                string statsLine =
                    $"{stats.FilesChanged} files changed, {stats.Insertions} insertions(+), {stats.Deletions} deletions(-)";
                return new List<string> {authorLine, string.Empty, statsLine};
            }).ToList();

        public static GitAuthorChangeStats ToGitChangeStats(this (string author, string stats) gitLogStdOutLinesEntry)
        {
            var statsStrings =
                gitLogStdOutLinesEntry.stats
                    .Split(',')
                    .Select(stat => stat.Trim())
                    .ToArray();

            return new GitAuthorChangeStats(
                gitLogStdOutLinesEntry.author, 
                Stat(statsStrings, "file"),
                Stat(statsStrings, "(+)"),
                Stat(statsStrings, "(-)"));

            int Stat(string[] statsStrings, string statDiscriminator)
            {
                var statString = StatString(statsStrings, statDiscriminator);
                return statString != null
                    ? int.Parse(statString.Split(' ')[0])
                    : 0;
            }

            string? StatString(string[] statsStrings, string statDiscriminator) =>
                statsStrings.SingleOrDefault(stat => stat.Contains(statDiscriminator));
        }
    }
}