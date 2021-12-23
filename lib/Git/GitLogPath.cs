using System.Text.RegularExpressions;

namespace Wikitools.Lib.Git;

public abstract partial record GitLogPath(string Path)
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
}