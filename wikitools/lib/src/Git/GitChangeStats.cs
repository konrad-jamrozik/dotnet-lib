using System;

namespace Wikitools.Lib.Git
{
    public record GitChangeStats(
        string Author,
        int FilesChanged,
        int Insertions,
        int Deletions)
    {
    }
}