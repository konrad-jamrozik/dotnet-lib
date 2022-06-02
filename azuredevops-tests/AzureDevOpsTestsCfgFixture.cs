using Wikitools.AzureDevOps.Config;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;

namespace Wikitools.AzureDevOps.Tests;

internal static class AzureDevOpsTestsCfgFixture
{
    public static IAzureDevOpsTestsCfg Cfg
    {
        get
        {
            var fs = new FileSystem();
            var cfg = new Configuration(fs);
            var adoTestsCfg = cfg.Load<IAzureDevOpsTestsCfg>();
            return adoTestsCfg;
        }
    }
}