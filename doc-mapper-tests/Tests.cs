using System.Text.RegularExpressions;

namespace DocMapper;

public class Tests
{
    public const int MaxExplorationDepth = 5;

    public const string RepoClonePathHomeDirSuffixAzureRestApiSpecs = "/repos/azure-rest-api-specs/";
    public const string RepoClonePathHomeDirInternalWiki = "/repos/internal.wiki/";

    public const string MapEntryPointUrl =
        "https://github.com/Azure/azure-rest-api-specs/blob/main/.github/PULL_REQUEST_TEMPLATE/control_plane_template.md";

    public Dictionary<string, string> UrlPrefixToClonePathMap = new Dictionary<string, string>();

    public Dictionary<string, string> UrlAliases = new Dictionary<string, string>
    {
        ["https://aka.ms/openapiportal"] = "https://portal.azure-devex-tools.com/"
    };

    public HashSet<string> LeafUrls = new HashSet<string>() { "https://portal.azure-devex-tools.com/" };

    public Queue<string> UrlsToExplore = new Queue<string>() {};

    public HashSet<string> ExploredUrls = new HashSet<string>() {};
    public HashSet<string> SkippedUrls = new HashSet<string>() {};

    // See also: https://learn.microsoft.com/en-us/rest/api/azure/devops/wiki/pages/get-page?view=azure-devops-rest-7.0&tabs=HTTP
    // kja TODO: write a separate test that will get all these URLs from the API and dump them to a local file to be read in Setup.
    // This might need to be done for multiple wikis.
    public readonly Dictionary<string, string> InternalWikiUrlPagePathToClonePathMap = new Dictionary<string, string>()
    {
        // https://dev.azure.com/azure-sdk/internal/_wiki/wikis/internal.wiki/208/OpenAPI-Hub-Adding-new-API-version
        ["208/OpenAPI-Hub-Adding-new-API-version"] =
            "Welcome-Service-Partners!/Management-Plane-SDKs-%2D-service-and-product-team-docs/OpenAPI-Hub%3A-Adding-new-API-version.md"
    };

    [SetUp]
    public void Setup()
    {
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        string repoClonePathAzureRestApiSpecs =
            Path.Join(homeDir, RepoClonePathHomeDirSuffixAzureRestApiSpecs).Replace("\\", "/");
        
        Assume.That(
            Directory.Exists(repoClonePathAzureRestApiSpecs),
            Is.True,
            $"Directory.Exists(repoClonePathAzureRestApiSpecs={repoClonePathAzureRestApiSpecs})");

        string repoClonePathInternalWiki =
            Path.Join(homeDir, RepoClonePathHomeDirInternalWiki).Replace("\\", "/");
        
        Assume.That(
            Directory.Exists(repoClonePathInternalWiki),
            Is.True,
            $"Directory.Exists(repoClonePathInternalWiki={repoClonePathInternalWiki})");

        UrlPrefixToClonePathMap = new Dictionary<string, string>
        {
            ["https://github.com/Azure/azure-rest-api-specs/blob/main/"] = repoClonePathAzureRestApiSpecs,
            ["https://dev.azure.com/azure-sdk/internal/_wiki/wikis/internal.wiki/"] = repoClonePathInternalWiki
        };
    }

    [Test]
    public void MapsDocs()
    {
        UrlsToExplore.Enqueue(MapEntryPointUrl);

        int currExplorationDepth = 0;

        while (UrlsToExplore.Count > 0 && currExplorationDepth < MaxExplorationDepth)
        {
            string urlToExplore = UrlsToExplore.Dequeue();
            Console.Out.WriteLine($"Exploring URL: {urlToExplore}");
            Console.Out.WriteLine($"----------------------------------------");

            if (SkippedUrls.Contains(urlToExplore))
            {
                Console.WriteLine($"Already skipped exploring urlToExplore={urlToExplore}. Skipping exploring the URL (again).");
                continue;
            }

            if (urlToExplore.StartsWith("https://aka.ms"))
            {
                string? matchingAkaMsAlias = UrlAliases.Keys.SingleOrDefault(akaMsAlias => urlToExplore.StartsWith(akaMsAlias));
                if (matchingAkaMsAlias == null)
                {
                    Console.WriteLine(
                        $"No aka.ms alias matched to a prefix of urlToExplore={urlToExplore}. Skipping exploring the URL.");
                    SkippedUrls.Add(urlToExplore);
                    continue;
                }
                else
                {
                    urlToExplore = UrlAliases[matchingAkaMsAlias];
                }
            }

            if (ExploredUrls.Contains(urlToExplore))
            {
                Console.WriteLine($"Already explored urlToExplore={urlToExplore}. Skipping exploring the URL.");
                continue;
            }

            if (LeafUrls.Contains(urlToExplore))
            {
                Console.WriteLine($"urlToExplore={urlToExplore} is a leaf URL. Skipping exploring the URL.");
                SkippedUrls.Add(urlToExplore);
                continue;
            }

            string? matchingClone =
                UrlPrefixToClonePathMap.Keys.SingleOrDefault(repoClonePath => urlToExplore.StartsWith(repoClonePath));
            if (matchingClone == null)
            {
                Console.WriteLine(
                    $"No repo clone path matched to a prefix of urlToExplore={urlToExplore}. Skipping exploring the URL.");
                SkippedUrls.Add(urlToExplore);
                continue;
            }

            string filePathInLocalClone = urlToExplore.Replace(matchingClone, UrlPrefixToClonePathMap[matchingClone]);

            if (filePathInLocalClone.Contains("internal.wiki"))
            {
                string? wikiUrlPagePath = InternalWikiUrlPagePathToClonePathMap.Keys.SingleOrDefault(wikiUrlPagePath => filePathInLocalClone.EndsWith(wikiUrlPagePath));
                if (wikiUrlPagePath == null)
                {
                    Console.WriteLine(
                        $"No known translation to clone .md path available for given internal.wiki page URL path. filePathInLocalClone={filePathInLocalClone}");
                    SkippedUrls.Add(urlToExplore);
                    continue;
                }
                else
                {
                    filePathInLocalClone = filePathInLocalClone.Replace(
                        wikiUrlPagePath,
                        InternalWikiUrlPagePathToClonePathMap[wikiUrlPagePath]);
                }
            }


            Assert.IsTrue(File.Exists(filePathInLocalClone), $"File.Exists(FilePathInLocalClone={filePathInLocalClone})");

            ExploreUrlsInFile(filePathInLocalClone);

            ExploredUrls.Add(urlToExplore);
            currExplorationDepth++;
        }

        Console.Out.WriteLine($"Exploration loop terminated. Exploration depth reached: {currExplorationDepth}/{MaxExplorationDepth}");

        Assert.Pass();
    }

    private void ExploreUrlsInFile(string filePathInLocalClone)
    {
        string fileAllText = File.ReadAllText(filePathInLocalClone);

        List<string> hyperlinkUrls = ExtractHyperlinkUrls(fileAllText);

        foreach (string hyperlinkUrl in hyperlinkUrls)
        {
            Console.Out.WriteLine(hyperlinkUrl);
        }
        Console.Out.WriteLine($"========================================");

        foreach (string hyperlinkUrl in hyperlinkUrls)
        {
            if (LeafUrls.Contains(hyperlinkUrl) || ExploredUrls.Contains(hyperlinkUrl) ||
                SkippedUrls.Contains(hyperlinkUrl))
            {
                // Do nothing - the URL is a leaf URL, already explored, or already skipped from exploration.
            }
            else
            {
                UrlsToExplore.Enqueue(hyperlinkUrl);
            }
        }
    }

    public static List<string> ExtractHyperlinkUrls(string markdownContent)
    {
        List<string> hyperlinks = new List<string>();

        // Regex patterns to match markdown hyperlinks
        string inlinePattern = @"\[(?:[^\[\]]*)\]\((.*?)\)"; // Inline links: [text](URL)
        string referencePattern = @"\[(?:[^\[\]]+)\]:\s*(.+)"; // Reference links: [text]: URL
        // kja TODO: need to handle relative wiki-internal URLS, as seen e.g. here:
        // https://dev.azure.com/azure-sdk/internal/_wiki/wikis/internal.wiki/208/OpenAPI-Hub-Adding-new-API-version

        // Match collections for both inline and reference hyperlinks
        MatchCollection inlineMatches = Regex.Matches(markdownContent, inlinePattern);
        MatchCollection referenceMatches = Regex.Matches(markdownContent, referencePattern);

        // Iterate through the matches and add URLs to the list
        foreach (Match match in inlineMatches)
        {
            hyperlinks.Add(match.Groups[1].Value);
        }

        foreach (Match match in referenceMatches)
        {
            hyperlinks.Add(match.Groups[1].Value.Trim());
        }

        return hyperlinks;
    }
}