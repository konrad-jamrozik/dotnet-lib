﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.Markdown;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Environment = Wikitools.Lib.OS.Environment;

namespace Wikitools;

// kj2 move all projects to C# 10 and use file-scoped namespaces
public static class Program
{
    public static async Task Main(string[] args)
    {
        ITimeline timeline = new Timeline();
        IOperatingSystem os = new WindowsOS();
        IFileSystem fs = new FileSystem();
        IEnvironment env = new Environment();

        var docsToWrite = DocsToWrite(timeline, os, fs, env);
        var outputSink  = Console.Out;
        await WriteAll(docsToWrite, outputSink);
    }

    private static MarkdownDocument[] DocsToWrite(
        ITimeline timeline,
        IOperatingSystem os,
        IFileSystem fs,
        IEnvironment env)
    {
        var cfg = new Configuration(fs).Read<WikitoolsCfg>();
        var wikiDecl = new AdoWikiWithStorageDeclare();
        var wiki = wikiDecl.AdoWikiWithStorage(
            timeline,
            fs,
            env,
            cfg.AzureDevOpsCfg.AdoWikiUri,
            cfg.AzureDevOpsCfg.AdoPatEnvVar,
            cfg.StorageDirPath);

        var pagesViewsStats = wiki.PagesStats(cfg.AdoWikiPageViewsForDays);

        var wikiToc = new WikiTableOfContents(
            new AdoWikiPagesPaths(fs.FileTree(cfg.GitRepoClonePath).Paths),
            pagesViewsStats);

        var docsToWrite = new MarkdownDocument[]
        {
            wikiToc
        };
        return docsToWrite;
    }

    private static Task WriteAll(MarkdownDocument[] docs, TextWriter textWriter) =>
        Task.WhenAll(docs.Select(doc => doc.WriteAsync(textWriter)).ToArray());
}

// kja 1 high-level todos:
// a) implement "top pages" report, which will show:
// - top 10 most edited pages last week. Might be less than 10 if not enough activity.
// - top 10 most viewed pages last week. Might be less than 10 if not enough activity.
// - Same as above, but for the last month.
// - Same as above, but for top 3 most active authors (with exclusions)
// - Add annotations (icons): Newly added, lots of traffic (use :fire: in the MD)
//   - Emojis: https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance?view=azure-devops#emoji
// b) make Program update all the reports in the local git clone, so that I can write a script
// that does:
//   git pull
//   wikitools.exe
//   git commit
//   git push

/*
 * Work progress milestones:
 * 11/21/2021:
 * - Writing out one wiki page TOC data, with stats from storage (>30 days).
 * 2/27/2021:
 * - Persisting wiki data to HDD as monthly json, allowing to report more than 30 days.
 * - Tooling for merging and splitting existing wiki data.
 * - To review these changes, explore from: Program, AdoWikiWithStorage
 * 1/18/2021:
 * - Markdown reports with data pulled from git.
 * - Markdown report with page view stats data pulled from ADO API.
 * - To review these changes, explore from: Program, GitAuthorsStatsReport, GitFilesStatsReport, MonthlyStatsReport, PagesViewsStatsReport
 * 12/29/2020:
 * - Work start. Reused GitRepository and AsyncLazy abstractions written in 2017.
 */
