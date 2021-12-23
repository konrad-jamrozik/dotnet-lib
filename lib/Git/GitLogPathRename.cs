using System.Linq;

namespace Wikitools.Lib.Git;

// kja make GitLogFilePathRename ctor private:
// https://stackoverflow.com/questions/69283960/is-it-possible-to-create-a-c-sharp-record-with-a-private-constructor
public record GitLogPathRename(
        string Path,
        string Prefix,
        string FromFragment,
        string ToFragment,
        string Suffix)
    : GitLogPath(Path)
{
    public string FromPath => Prefix + FromFragment + AdjustedSuffix(FromFragment);

    public string ToPath => Prefix + ToFragment + AdjustedSuffix(ToFragment);


    // Need to skip beginning of suffix, which is "/", if the fragment is empty.
    // This is because the input format would result in double "/" otherwise.
    // Example:
    // abc/{ => newdir/newsubdir}/ghi/foo.md
    private string AdjustedSuffix(string fragment)
        => new string(Suffix.Skip(fragment == string.Empty ? 1 : 0).ToArray());
}