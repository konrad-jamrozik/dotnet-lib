using System;
using Wikitools.Lib.Contracts;

namespace Wikitools.Lib.Json
{
    public interface IConfiguration
    {
        public const string ConfigSuffix = "Cfg";

        public static string FileName(Type cfg) => FileName(cfg.Name);

        public static string FileName(string cfgName)
        {
            Contract.Assert(cfgName.EndsWith(ConfigSuffix));
            return $"{cfgName[..^ConfigSuffix.Length]}_config.json";
        }
    }
}