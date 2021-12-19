namespace Wikitools.Lib.Git;

public record GitLogFilePathRename(
        string Path,
        string Prefix,
        string FromFileName,
        string ToFileName)
    : GitLogFilePath(Path)
{
    public string FromPath => Prefix + FromFileName;
    public string ToPath => Prefix + ToFileName;
}