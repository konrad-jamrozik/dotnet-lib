using System.Linq;

namespace Wikitools.Lib.Git;

public partial record GitLogPath
{
    private record GitLogPathRename(
            string Path,
            string Prefix,
            string FromFragment,
            string ToFragment,
            string Suffix)
        : GitLogPath(Path)
    {
        public override string ToString()
            => Path;

        public new string FromPath => Prefix + FromFragment + AdjustedSuffix(FromFragment);

        public new string ToPath => Prefix + ToFragment + AdjustedSuffix(ToFragment);

        // Explanation why a call to .Skip is made:
        // Need to skip beginning of suffix, which is "/", if the fragment is empty.
        // This is because the input format would result in double "/" otherwise.
        // Example:
        // abc/{ => newdir/newsubdir}/ghi/foo.md
        private string AdjustedSuffix(string fragment)
            => new string(Suffix.Skip(fragment == string.Empty ? 1 : 0).ToArray());
    }
}