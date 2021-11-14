using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wikitools.AzureDevOps
{
    /// <summary>
    /// Represents a sorted set of file paths of pages of ADO wiki, filtered out from the input GitClonePaths.
    ///
    /// Assumptions:
    /// - GitClonePaths is a collection of all file paths obtained from a root of given ADO wiki git clone root.
    /// </summary>
    public record AdoWikiPagesPaths(IEnumerable<string> GitClonePaths) : IEnumerable<string>
    {
        // kj2 this should be parameterized
        public const string WikiPagesFolder = "wiki";
        public const string WikiPagesPrefix = WikiPagesFolder + "\\";

        private IEnumerable<string> PagesPaths
            => new SortedSet<string>(
                GitClonePaths
                    .Where(
                        path =>
                            // Take paths only from within wiki pages folder
                            path.StartsWith(WikiPagesFolder)
                            // Filter out metadata directories and files
                            && !Regex.Match(path, @"\\.attachments|\\\.order").Success)
                    // Strip from each page path the wiki pages folder prefix.
                    .Select(path => path.Substring(WikiPagesPrefix.Length))
                );

        public IEnumerator<string> GetEnumerator() => PagesPaths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}