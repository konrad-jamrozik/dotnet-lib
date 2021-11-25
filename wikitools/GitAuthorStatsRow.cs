namespace Wikitools;

public record GitAuthorStatsRow(
    int Place,
    string AuthorName,
    int FilesChanges,
    int Insertions,
    int Deletions);