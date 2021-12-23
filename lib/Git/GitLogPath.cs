using System.Text.RegularExpressions;

namespace Wikitools.Lib.Git;

// kja make GitLogFilePath ctor private:
// https://stackoverflow.com/questions/69283960/is-it-possible-to-create-a-c-sharp-record-with-a-private-constructor
public record GitLogPath(string Path)
{
    public static implicit operator GitLogPath(string filePath) => new GitLogPath(filePath);

    public static GitLogPath From(string path)
        => TryParseRename(path) ?? new GitLogPath(path);

    public override string ToString()
        => Path;

    private static GitLogPathRename? TryParseRename(string path)
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