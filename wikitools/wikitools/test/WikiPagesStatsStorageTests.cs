using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tests.Json;
using Xunit;
using Data = Wikitools.Tests.WikiPagesStatsStorageTestsData;
namespace Wikitools.Tests
{
    // kja 1 remaining merge tests to add
    // Some test checking which page rename takes precedence, both in correct ((prev,curr)) and flipped ((curr,prev)) ordering.
    //   Do this test by passing the same data but flipped current/previous.
    public class WikiPagesStatsStorageTests
    {
        // @formatter:off
        [Fact] public void PageStatsEmpty()                  => Verify(Data.PageStatsEmpty);
        [Fact] public void PageStatsYearWrap()               => Verify(Data.PageStatsYearWrap);
        [Fact] public void PageStatsBeforeYearWrap()         => Verify(Data.PageStatsBeforeYearWrap);
        [Fact] public void PageStatsPreviousMonthOnly()      => Verify(Data.PageStatsPreviousMonthOnly);
        [Fact] public void PageStats()                       => Verify(Data.PageStats);
        [Fact] public void PageStatsSameDay()                => Verify(Data.PageStatsSameDay);
        [Fact] public void PageStatsSamePreviousDay()        => Verify(Data.PageStatsSamePreviousDay);
        [Fact] public void PageStatsSameDayDifferentCounts() => Verify(Data.PageStatsSameDayDifferentCounts);
        [Fact] public void PageStatsSameMonth()              => Verify(Data.PageStatsSameMonth);
        [Fact] public void PageStatsUnorderedDayStats()      => Verify(Data.PageStatsUnorderedDayStats);
        // kja [Fact] public void PageStatsRenamedToNewPath()       => Verify(Data.PageStatsRenamedToNewPath);
        // @formatter:on

        // kja 3 consider moving this verify logic onto the data type itself.
        private static void Verify(WikiPagesStatsTestData data)
        {
            // Act - Split({foo, bar})
            (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth)? split =
                VerifySplitByMonth(data, data.SplitByMonthThrows ? typeof(ArgumentException) : null);

            // Act - Merge(foo, bar)
            WikiPageStats[]? merged = VerifyMerge(data, data.MergeThrows ? typeof(ArgumentException) : null);

            if (!data.SplitByMonthThrows)
            {
                // Act - Split(Split({foo, bar}))
                var (previousPreviousMonth, currentPreviousMonth) = WikiPagesStatsStorage.SplitByMonth(split!.Value.previousMonth, data.Date);
                var (previousCurrentMonth, currentCurrentMonth) = WikiPagesStatsStorage.SplitByMonth(split!.Value.currentMonth, data.Date);
                new JsonDiffAssertion(split!.Value.previousMonth, previousPreviousMonth).Assert();
                Assert.DoesNotContain(currentPreviousMonth, ps => ps.DayStats.Any());
                Assert.DoesNotContain(previousCurrentMonth, ps => ps.DayStats.Any());
                new JsonDiffAssertion(split!.Value.currentMonth,  currentCurrentMonth).Assert();
            }

            if (!data.MergeThrows)
            {
                // Act - Merge(Merge(foo, bar), Merge(foo, bar))
                var mergedTwice = WikiPagesStatsStorage.Merge(merged!, merged!);
                new JsonDiffAssertion(merged!, mergedTwice).Assert();
            }

            if (data.MergeThrows || data.SplitByMonthThrows) 
                return;

            // kja observation: this is not going to work if page was renamed.
            // Merge will lose the previous month name, and splitting back will do so too.
            // Act - Split(Merge(foo, bar)) == (foo, bar)
            var (previousMonthUnmerged, currentMonthUnmerged) =
                WikiPagesStatsStorage.SplitByMonth(merged!, data.Date);
            new JsonDiffAssertion(data.PreviousMonth, previousMonthUnmerged).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonthUnmerged).Assert();

            // Act - Merge(Split({foo, bar})) == Merge(foo, bar)
            var mergedSplit = WikiPagesStatsStorage.Merge(split!.Value.previousMonth, split!.Value.currentMonth);
            new JsonDiffAssertion(data.MergedPagesStats, mergedSplit).Assert();
        }

        private static (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth)? VerifySplitByMonth(
            WikiPagesStatsTestData data, Type? excType) =>
            Verification.VerifyStruct(VerifySplitByMonth, data, excType);

        private static WikiPageStats[]? VerifyMerge(WikiPagesStatsTestData data, Type? excType) =>
            Verification.Verify<WikiPagesStatsTestData, WikiPageStats[]?>(VerifyMerge,
                data,
                excType);

        private static (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth) VerifySplitByMonth(
            WikiPagesStatsTestData data)
        {
            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(data.AllPagesStats, data.Date);
            new JsonDiffAssertion(data.PreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonth).Assert();
            return (previousMonth, currentMonth);
        }

        private static WikiPageStats[] VerifyMerge(WikiPagesStatsTestData data)
        {
            // Act
            var merged = WikiPagesStatsStorage.Merge(data.PreviousMonth, data.CurrentMonth);
            new JsonDiffAssertion(data.MergedPagesStats, merged).Assert();
            return merged;
        }
    }
}