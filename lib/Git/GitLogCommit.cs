using System;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git;

public record GitLogCommit(string Author, DateTime Date, GitLogCommit.Numstat[] Stats)
{
    public GitLogCommit(string[] commit) : this(
        GetAuthor(commit),
        GetDate(commit),
        GetNumstats(commit)) { }

    private static string GetAuthor(string[] commit) => commit[0];

    private static DateTime GetDate(string[] commit) => DateTime.Parse(commit[1]);

    private static Numstat[] GetNumstats(string[] commit) =>
        commit[2..].Select(line => new Numstat(line)).ToArray();

    public record Numstat(int Insertions, int Deletions, string FilePath)
    {
        public Numstat(string line) : this(Parse(line)) { }

        private Numstat((int insertions, int deletions, string filePath) data) : this(
            data.insertions,
            data.deletions,
            data.filePath) { }

        private static (int insertions, int deletions, string filePath) Parse(string line)
        {
            var split      = line.Split('\t');
            var insertions = int.Parse(split[0].Replace("-", "0"));
            var deletions  = int.Parse(split[1].Replace("-", "0"));
            var filePath   = split[2];
            return (insertions, deletions, filePath);
        }
    }

    public static GitLogCommit[] FilterCommits(
        GitLogCommit[] commits,
        (DateDay sinceDay, DateDay untilDay) daySpan)
    {
        commits = commits
            .Where(
                commit => daySpan.sinceDay.CompareTo(commit.Date) <= 0
                          && daySpan.untilDay.CompareTo(commit.Date) >= 0)
            .ToArray();

        return commits;
    }
}