namespace Wikitools;

public record WikiPageLink(string WikiPageStatsPath)
{
    public static WikiPageLink FromFileSystemPath(string path)
        => new WikiPageLink(AzureDevOps.WikiPageStatsPath.FromFileSystemPath(path).Path);

    public override string ToString()
        => $"[{WikiPageStatsPath}]({ConvertPathToWikiLink(WikiPageStatsPath)})";

    /// <summary>
    /// Converts the 'path', which is in format
    /// Wikitools.AzureDevOps.WikiPageStatsPath.WikiPageStatsPath,
    /// to a link that ADO Wiki understands as an absolute path to a page in given wiki.
    ///
    /// Reference:
    /// See "Supported links for Wiki" / "Absolute path of Wiki pages" in:
    /// <a href="https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops#links" />
    /// </summary>
    private static string ConvertPathToWikiLink(string path)
    {
        path = path.Replace("-","%2D").Replace(" ","-").Replace("(", @"\(").Replace(")", @"\)");
        return path;
    }
}