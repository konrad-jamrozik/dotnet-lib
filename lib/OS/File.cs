namespace Wikitools.Lib.OS;

public record File(IFileSystem FileSystem, string Path)
{
    public File(Dir dir, string fileName) : this(dir.FileSystem, dir.JoinPath(fileName))
    {
    }

    public string Name => System.IO.Path.GetFileName(Path);
}