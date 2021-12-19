namespace Wikitools.Lib.Git;

public record GitLogFilePath(string Path)
{
    public static implicit operator GitLogFilePath(string filePath) => new GitLogFilePath(filePath);

    // kj2 this op should not be necessary: all users should operate on GitLogFilePath instead.
    public static implicit operator string(GitLogFilePath gitLogFilePath) => gitLogFilePath.Path;

    public static GitLogFilePath From(string path)
    {
        var gitLogFilePath = new GitLogFilePath(path);
        if (gitLogFilePath.IsRename)
            gitLogFilePath = new GitLogFilePathRename(gitLogFilePath);
        return gitLogFilePath;
    }
    public bool IsRename = false; // kja to implement. And fixup From to not be silly.
}