namespace Wikitools.Lib.Git;

public partial record GitLogPath
{
    // Nested class used to prevent creation of records via ctor. Based on:
    // https://stackoverflow.com/questions/64309291/how-do-i-define-additional-initialization-logic-for-the-positional-record
    private record GitLogPathFile(string Path) : GitLogPath(Path)
    {
        public override string ToString()
            => Path;
    }
}