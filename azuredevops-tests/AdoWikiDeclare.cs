using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps.Tests;

public static class AdoWikiDeclare
{
    public static IAdoWiki New(IAzureDevOpsTestsCfg adoTestsCfg, int? pageViewsForDaysMax = null)
    {
        var env = new Environment();
        IAdoWiki wiki = new AdoWiki(
            adoTestsCfg.AzureDevOpsCfg().AdoWikiUri(),
            adoTestsCfg.AzureDevOpsCfg().AdoPatEnvVar(),
            env);
        wiki = new AdoWikiWithPreconditionChecks(wiki, pageViewsForDaysMax);
        return wiki;
    }
}