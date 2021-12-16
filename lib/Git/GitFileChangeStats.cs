namespace Wikitools.Lib.Git;

public record GitFileChangeStats(
    string FilePath,
    int Insertions,
    int Deletions) { }