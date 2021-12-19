namespace Wikitools.Lib.Git;

// kj2 make GitLogFilePathRename ctor private:
// https://stackoverflow.com/questions/69283960/is-it-possible-to-create-a-c-sharp-record-with-a-private-constructor
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