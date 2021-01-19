using System;
using System.Linq;
using Microsoft.TeamFoundation.Wiki.WebApi;

namespace Wikitools.AzureDevOps
{
    public record WikiPageStats(string Path, int Id, WikiPageStats.Stat[] Stats)
    {
        public WikiPageStats(WikiPageDetail pageDetail) : this(
            pageDetail.Path,
            pageDetail.Id,
            GetStats(pageDetail)) { }

        private static Stat[] GetStats(WikiPageDetail pageDetail) =>
            pageDetail.ViewStats?.Select(dayStat => new Stat(dayStat.Count, dayStat.Day.ToUniversalTime())).ToArray()
            ?? Array.Empty<Stat>();

        public record Stat(int Count, DateTime Day) { }
    }
}