using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Wikitools.AzureDevOps
{
    /// <summary>
    /// Represents a sorted set of file paths of pages to ADO wiki, filtered out from the input GitCloneRootPaths.
    ///
    /// Assumptions:
    /// - GitCloneRootPaths is a collection of all file paths obtained from a root of given ADO wiki git clone root.
    /// </summary>
    public record AdoWikiPagesPaths(IEnumerable<string> GitCloneRootPaths) : IEnumerable<string>
    {
        // kj2 this should be parameterized
        public const string WikiPagesFolder = "wiki";
        public const string WikiPagesPrefix = WikiPagesFolder + "\\";

        private IEnumerable<string> PagePaths
            => new SortedSet<string>(
                GitCloneRootPaths
                    .Where(
                        path =>
                            // Take paths only from within wiki pages folder
                            path.StartsWith(WikiPagesFolder)
                            // Filter out metadata directories and files
                            && !Regex.Match(path, @"\\.attachments|\\\.order").Success)
                    // Strip the wiki pages folder
                    .Select(path => path.Substring(WikiPagesPrefix.Length))
                );

        public IEnumerator<string> GetEnumerator() => PagePaths.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}