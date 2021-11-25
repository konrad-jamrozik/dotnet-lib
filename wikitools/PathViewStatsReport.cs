using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record PathViewStatsReport : MarkdownDocument
    {
        public const string DescriptionFormat =
            "Path views since last {0} days as of {1}. Total wiki pages: {2}";

        public PathViewStatsReport(
            ITimeline timeline,
            int pageViewsForDays,
            PathViewStats[] stats) : base(
            GetContent(timeline, pageViewsForDays, stats)) { }

        private static object[] GetContent(
            ITimeline timeline,
            int pageViewsForDays,
            PathViewStats[] stats)
            =>
                new object[]
                {
                    string.Format(DescriptionFormat, pageViewsForDays, timeline.UtcNow, stats.Length),
                    "",
                    PathViewStats.TabularData(stats)
                };
    }
}