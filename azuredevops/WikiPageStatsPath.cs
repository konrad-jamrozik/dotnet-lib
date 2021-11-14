using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using MoreLinq;

namespace Wikitools.AzureDevOps
{
    // kj2 make the WikiPageStats.Path param be of this type
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
    /// - Special characters of "/","\" and "#" withing path segments are not allowed.
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
        private const char Separator = '/';

        /// <summary>
        /// Converts a file system path to a markdown file with contents of given ADO wiki page
        /// to a corresponding WikiPageStatsPath.
        /// </summary>
        public static WikiPageStatsPath FromFileSystemPath(string path)
        {
            // kja 6 curr work. Need to:
            // - write tests
            // - convert "-" to " "
            // - add "/" att the beginning
            // - unescape special chars
            // - sort again once all of this is done
            // - example target path: proj/proj-Customer-Support/proj-Customer-FAQ-re-Kusto-Schema-(08%2D07%2D2020)
            //   - the ending here is "(08-07-2020)"

            var processedPath = path.Replace(System.IO.Path.DirectorySeparatorChar, Separator);
            processedPath = StripMarkdownFileExtension(processedPath);
            return new WikiPageStatsPath(processedPath);
        }

        public static IEnumerable<string> SplitPath(string path) => path.Split(Separator);

        public static explicit operator string(WikiPageStatsPath path)
            => path.Path;

        private static string StripMarkdownFileExtension(string path)
            => path[..^".md".Length];
    }
}