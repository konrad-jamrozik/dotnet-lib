﻿using Wikitools.AzureDevOps;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;

namespace Wikitools
{
    public static class Declare
    {
        public static GitLog GitLog(IOperatingSystem os, string gitRepoDirPath, string gitExecutablePath)
        {
            var repo = new GitRepository(
                new GitBashShell(os, gitExecutablePath),
                gitRepoDirPath
            );
            var gitLog = new GitLog(repo);
            return gitLog;
        }

        public static AdoWiki Wiki(IAdoApi adoApi, string wikiUri, string patEnvVar) =>
            new(adoApi, new AdoWikiUri(wikiUri), patEnvVar);
    }
}