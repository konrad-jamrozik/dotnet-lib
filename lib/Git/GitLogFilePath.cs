namespace Wikitools.Lib.Git;

public record GitLogFilePath(string Path)
{
    public static implicit operator GitLogFilePath(string filePath) => new GitLogFilePath(filePath);

    // kj2 this op should not be necessary: all users should operate on GitLogFilePath instead.
    public static implicit operator string(GitLogFilePath gitLogFilePath) => gitLogFilePath.Path;

    public bool IsRename = false; // kja to implement.
}