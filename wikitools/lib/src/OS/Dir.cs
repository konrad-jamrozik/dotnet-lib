namespace Wikitools.Lib.OS
{
    public record Dir(IFileSystem FileSystem, string Path)
    {
        public bool Exists() => FileSystem.DirectoryExists(Path);
    }
}
