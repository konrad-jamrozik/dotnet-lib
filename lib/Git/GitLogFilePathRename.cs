namespace Wikitools.Lib.Git;

// kja to implement
public record GitLogFilePathRename : GitLogFilePath
{
    public readonly string FromPath;
    public readonly string ToPath;

    public GitLogFilePathRename(string path) : base(path)
    {
        var (from, to) = ParseRename(path);
        FromPath = from;
        ToPath = to;
    }

    private (string from, string to) ParseRename(string path)
    {
        throw new System.NotImplementedException();
    }
}