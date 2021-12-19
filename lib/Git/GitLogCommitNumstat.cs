using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Git;

public partial record GitLogCommit
{
    public record Numstat
    {
        public int Insertions { get; }
        public int Deletions { get; }
        public GitLogFilePath FilePath { get; }

        public Numstat(int insertions, int deletions, string filePath) : this(
            insertions,
            deletions,
            GitLogFilePath.From(filePath)) { }

        public Numstat(int insertions, int deletions, GitLogFilePath filePath)
        {
            // Note this setup of invariant checks in ctor has some problems.
            // Details here: https://github.com/dotnet/csharplang/issues/4453#issuecomment-782807066
            // kja fails WritesMonthlyStatsReport on this
            Contract.Assert(
                filePath is not GitLogFilePathRename || insertions == 0 && deletions == 0);
            Insertions = insertions;
            Deletions = deletions;
            FilePath = filePath;
        }

        public Numstat(string line) : this(Parse(line)) { }

        private Numstat((int insertions, int deletions, string filePath) data) : this(
            data.insertions,
            data.deletions,
            GitLogFilePath.From(data.filePath)) { }

        private static (int insertions, int deletions, string filePath) Parse(string line)
        {
            var split      = line.Split('\t');
            var insertions = int.Parse(split[0].Replace("-", "0"));
            var deletions  = int.Parse(split[1].Replace("-", "0"));
            var filePath   = split[2];
            return (insertions, deletions, filePath);
        }
    }

}