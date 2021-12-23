﻿using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git;

public partial record GitLogCommit
{
    public record Numstat
    {
        public int Insertions { get; }
        public int Deletions { get; }
        public GitLogPath Path { get; init; }

        public Numstat(int insertions, int deletions, string filePath) : this(
            insertions,
            deletions,
            GitLogPath.From(filePath)) { }

        public Numstat(int insertions, int deletions, GitLogPath path)
        {
            // Note this setup of invariant checks in ctor has some problems.
            // Details here: https://github.com/dotnet/csharplang/issues/4453#issuecomment-782807066
            Insertions = insertions;
            Deletions = deletions;
            Path = path;
        }

        public Numstat(string line) : this(Parse(line)) { }

        public static ILookup<string, Numstat> ByFileNameAfterRenames(IList<Numstat> numstats)
        {
            var numstatsLookup = numstats
                .Select(
                    stats => stats.Path is GitLogPathRename filePath
                        ? stats with
                        {
                            // Stats in file rename entries count towards the
                            // resulting file name (i.e. having "ToPath").
                            Path = new GitLogPath(filePath.ToPath)
                        }
                        : stats)
                .ToLookup(stats => stats.Path.ToString());

            numstatsLookup = RenameMap(numstats).Apply(numstatsLookup);
            return numstatsLookup;
        }

        private static RenameMap RenameMap(IList<Numstat> numstats)
        {
            // Assert: fileStats are in reverse-chronological order.

            var gitRenames = numstats
                .Where(stats => stats.Path is GitLogPathRename)
                .Select(stats => (GitLogPathRename)stats.Path);

            var sortedRenames = gitRenames
                .Select(rename => (rename.FromPath, rename.ToPath))
                // Reversing here so that renames are in chronological order.
                // This assumes that fileStats were in reverse chronological order.
                .Reverse();
        
            var renameMap = new RenameMap(sortedRenames);

            return renameMap;
        }

        private Numstat((int insertions, int deletions, string filePath) data) : this(
            data.insertions,
            data.deletions,
            GitLogPath.From(data.filePath)) { }

        private static (int insertions, int deletions, string filePath) Parse(string line)
        {
            var split      = line.Split('\t');
            var insertions = int.Parse(split[0].Replace("-", "0"));
            var deletions  = int.Parse(split[1].Replace("-", "0"));
            var filePath   = split[2];
            return (insertions, deletions, filePath);
        }
    }

}