using System.Text.RegularExpressions;

namespace Wikitools.Lib.Git;

// kj2 make GitLogFilePath ctor private:
// https://stackoverflow.com/questions/69283960/is-it-possible-to-create-a-c-sharp-record-with-a-private-constructor
public record GitLogFilePath(string Path)
{
    public static implicit operator GitLogFilePath(string filePath) => new GitLogFilePath(filePath);

    // kj2 this op should not be necessary: all users should operate on GitLogFilePath instead.
    public static implicit operator string(GitLogFilePath gitLogFilePath) => gitLogFilePath.Path;

    public static GitLogFilePath From(string path)
        => TryParseRename(path) ?? new GitLogFilePath(path);

    public override string ToString()
        => Path;

    private static GitLogFilePathRename? TryParseRename(string path)
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
            ? new GitLogFilePathRename(
                path,
                match.Groups[1].Value,
                match.Groups[2].Value,
                match.Groups[3].Value,
                match.Groups[4].Value)
            : null;
    }
}