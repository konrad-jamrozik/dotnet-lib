using System;
using System.Linq;

namespace Wikitools.Lib.Git;

public partial record GitLogCommit(string Author, DateTime Date, GitLogCommit.Numstat[] Stats)
{
    public GitLogCommit(string[] commit) : this(
        GetAuthor(commit),
        GetDate(commit),
        GetNumstats(commit)) { }

    private static string GetAuthor(string[] commit) => commit[0];

    private static DateTime GetDate(string[] commit) => DateTime.Parse(commit[1]);

    private static Numstat[] GetNumstats(string[] commit) =>
        commit[2..].Select(line => new Numstat(line)).ToArray();


}