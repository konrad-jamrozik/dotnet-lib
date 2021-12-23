using System.Linq;
using System.Text.RegularExpressions;

namespace Wikitools.Lib.Git;

public abstract record GitLogPath(string Path)
{
    public static GitLogPath From(string path)
        => TryParseRename(path) ?? new GitLogPathFile(path);

    public override string ToString()
        => Path;

    public bool IsRename => this is GitLogPathRename;

    public string FromPath => this switch
    {
        GitLogPathRename rename => rename.FromPath,
        _ => Path
    };

    public string ToPath => this switch
    {
        GitLogPathRename rename => rename.ToPath,
        _ => Path
    };

    private static GitLogPath? TryParseRename(string path)
    {
        // Example input paths:
        //
        // abc/def/{bar.md => qux.md}
        // abc/{to/rem{ove => to/a}dd}/def/foo.md
        // abc/{to/r{emove => }/def/foo.md
        // abc/{ => to/a}dd}/def/foo.md
        // { => to/a}dd}/def/foo.md
        var match = Regex.Match(path, "(.*?){(\\S*) => (\\S*)}(.*)");

        return match.Success
            ? new GitLogPathRename(
                path,
                match.Groups[1].Value,
                match.Groups[2].Value,
                match.Groups[3].Value,
                match.Groups[4].Value)
            : null;
    }

    // Nested class used to prevent creation of records via ctor. Based on:
    // https://stackoverflow.com/questions/64309291/how-do-i-define-additional-initialization-logic-for-the-positional-record
    private record GitLogPathFile(string Path) : GitLogPath(Path)
    {
        public override string ToString()
            => Path;
    }

    private record GitLogPathRename(
            string Path,
            string Prefix,
            string FromFragment,
            string ToFragment,
            string Suffix)
        : GitLogPath(Path)
    {
        public override string ToString()
            => Path;

        public new string FromPath => Prefix + FromFragment + AdjustedSuffix(FromFragment);

        public new string ToPath => Prefix + ToFragment + AdjustedSuffix(ToFragment);

        // Explanation why a call to .Skip is made:
        // Need to skip beginning of suffix, which is "/", if the fragment is empty.
        // This is because the input format would result in double "/" otherwise.
        // Example:
        // abc/{ => newdir/newsubdir}/ghi/foo.md
        private string AdjustedSuffix(string fragment)
            => new string(Suffix.Skip(fragment == string.Empty ? 1 : 0).ToArray());
    }
}