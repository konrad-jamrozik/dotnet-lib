using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DocMapper;

public class Tests
{
    public const string RepoClonePathHomeDirSuffixAzureRestApiSpecs = "/repos/azure-rest-api-specs/";

    public const string MapEntryPointUrl =
        "https://github.com/Azure/azure-rest-api-specs/blob/main/.github/PULL_REQUEST_TEMPLATE/control_plane_template.md";

    public Dictionary<string, string> UrlPrefixToClonePathMap = new Dictionary<string, string>();

    public Dictionary<string, string> UrlAliases = new Dictionary<string, string>
    {
        ["https://aka.ms/openapiportal"] = "https://portal.azure-devex-tools.com/"
    };

    public HashSet<string> LeafAliases = new HashSet<string>() { "https://portal.azure-devex-tools.com/" };

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

        UrlPrefixToClonePathMap = new Dictionary<string, string>
        {
            ["https://github.com/Azure/azure-rest-api-specs/blob/main/"] = repoClonePathAzureRestApiSpecs
        };
    }

    [Test]
    public void MapsDocs()
    {
        string? matchingKey = UrlPrefixToClonePathMap.Keys.SingleOrDefault(key => MapEntryPointUrl.StartsWith(key));
        if (matchingKey == null)
            Assert.Fail($"No repo clone path matched to a prefix of MapEntryPointUrl={MapEntryPointUrl}");

        string filePathInLocalClone = MapEntryPointUrl.Replace(matchingKey!, UrlPrefixToClonePathMap[matchingKey!]);

        Assert.IsTrue(File.Exists(filePathInLocalClone), $"File.Exists(FilePathInLocalClone={filePathInLocalClone})");

        string fileAllText = File.ReadAllText(filePathInLocalClone);

        List<string> hyperlinkUrls = ExtractHyperlinkUrls(fileAllText);

        foreach (var hyperlinkUrl in hyperlinkUrls)
        {
            Console.Out.WriteLine(hyperlinkUrl);
        }

        Assert.Pass();
    }

    public static List<string> ExtractHyperlinkUrls(string markdownContent)
    {
        List<string> hyperlinks = new List<string>();

        // Regex patterns to match markdown hyperlinks
        string inlinePattern = @"\[(?:[^\[\]]*)\]\((.*?)\)"; // Inline links: [text](URL)
        string referencePattern = @"\[(?:[^\[\]]+)\]:\s*(.+)"; // Reference links: [text]: URL

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