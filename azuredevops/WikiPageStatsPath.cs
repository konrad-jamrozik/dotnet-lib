using System.Net;

namespace Wikitools.AzureDevOps;

// kj2-toc make the WikiPageStats.Path param be of this type
/// <summary>
/// Represents a Wikitools.AzureDevOps.WikiPageStats.Path.
///
/// Information below is based on empirical observations.
///
/// Examples of valid paths:
///
///   /introduction
///   /fooCompany/bar project/Onboarding new customer/Step 3: configuring environment
///   /weirdPath/of/hereItIs: - ? - vault no. "123" .exe
///
/// Observed rules:
/// - The path separators are "/".
/// - The path always starts with a separator.
/// - Special characters of "/","\" and "#" within path segments are not allowed.
/// - There cannot be any spaces at end of any path segment
///   (they will get trimmed by wiki when saving the path).
/// - There cannot be periods at the start or end of any path segment (same reason as above).
/// - A path cannot end with ".md"
///   - This is likely because in the repository the pages are saved as .md (markdown) files.
/// - Other various special characters are allowed.
/// - No special characters are escaped.
/// </summary>
public record WikiPageStatsPath(string Path)
{
    /// <summary>
    /// See class comment.
    /// </summary>
    public const string Separator = "/";

    /// <summary>
    /// Converts a file system path to a markdown file with contents of given ADO wiki page
    /// to a corresponding WikiPageStatsPath.
    /// </summary>
    public static WikiPageStatsPath FromFileSystemPath(string path)
    {
        var processedPath = path.Replace(System.IO.Path.DirectorySeparatorChar.ToString(), Separator);
        processedPath = Separator + StripMarkdownFileExtension(processedPath);
        processedPath = processedPath.Replace('-', ' ');
        // This ensures that UrlDecode will preserve the + signs instead of converting them to spaces.
        processedPath = processedPath.Replace("+", "%2B");
        processedPath = WebUtility.UrlDecode(processedPath);
        return new WikiPageStatsPath(processedPath);
    }

    public static explicit operator string(WikiPageStatsPath path)
        => path.Path;

    private static string StripMarkdownFileExtension(string path)
        => path[..^".md".Length];
}