using Wikitools.Lib.Markdown;
using Wikitools.Lib.Primitives;

namespace Wikitools
{
    public record PageViewStatsReport : MarkdownDocument
    {
        public const string DescriptionFormat =
            "Page views since last {0} days as of {1}. Total wiki pages: {2}";

        public PageViewStatsReport(
            ITimeline timeline,
            int pageViewsForDays,
            PageViewStats[] stats) : base(
            GetContent(timeline, pageViewsForDays, stats)) { }

        private static object[] GetContent(
            ITimeline timeline,
            int pageViewsForDays,
            PageViewStats[] stats)
            =>
                new object[]
                {
                    string.Format(DescriptionFormat, pageViewsForDays, timeline.UtcNow, stats.Length),
                    "",
                    PageViewStats.TabularData(stats)
                };
    }
}