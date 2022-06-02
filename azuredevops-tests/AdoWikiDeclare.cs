using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.AzureDevOps.Tests;

public static class AdoWikiDeclare
{
    public static IAdoWiki New(IAzureDevOpsTestsCfg adoTestsCfg, DateDay currentDay)
    {
        var env = new Environment();
        IAdoWiki wiki = new AdoWiki(
            adoTestsCfg.AzureDevOpsCfg().AdoWikiUri(),
            adoTestsCfg.AzureDevOpsCfg().AdoPatEnvVar(),
            env,
            currentDay);
        wiki = new AdoWikiWithPreconditionChecks(wiki);
        return wiki;
    }
}