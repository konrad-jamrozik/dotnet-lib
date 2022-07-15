using System;
using Microsoft.VisualStudio.Services.CircuitBreaker;
using Wikitools.AzureDevOps;
using Wikitools.Config;
using Wikitools.Lib.Configuration;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Xunit;
using Xunit.Abstractions;

namespace Wikitools.Tests;

public class Tools
{
    private readonly ITestOutputHelper _testOut;

    public Tools(ITestOutputHelper testOut)
    {
        _testOut = testOut;
    }

    [Fact(Skip = "For one-off experiments")]
    public void Scratchpad()
    {
        DaySpan x = new DaySpan(new DateDay(DateTime.UtcNow), new DateDay(DateTime.UtcNow));
        _testOut.WriteLine(x.ToString());
        var y = new WikiPageStats.DayStat(40, new DateDay(DateTime.Today));
        _testOut.WriteLine(y.ToString());
    }

    [Fact(Skip = "Tool to be used manually")]
    public void WriteOutGitRepoClonePaths()
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).Load<IWikitoolsCfg>();
        var clonePath = cfg.GitRepoClonePath();
        _testOut.WriteLine("Clone path: " + clonePath);
        var filteredPaths = new AdoWikiPagesPaths(fs.FileTree(clonePath).Paths);
        foreach (var fileTreePath in filteredPaths)
        {
            _testOut.WriteLine(fileTreePath);
        }
    }
}