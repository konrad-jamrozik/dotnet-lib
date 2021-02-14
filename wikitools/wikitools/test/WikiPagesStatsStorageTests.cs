using System;
using System.Linq;
using MoreLinq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
    // kja NEXT: add Merge tests.
    // Some tests:
    // DONE Split(x,x) = x, where x is only for one month
    // DONE Merge(Split(x,y)) == (x,y)
    // DONE Split(Merge(x,y)) == (x,y)
    // DONE Merge(x,x) == x
    // DONE Merge(page1, page2) == stats for both page1 and page2
    // Merge(page1month1, page1month2) == stats for both months for page1
    // Merge(page1day1, page1day2) == stats for both days for page1
    // DONE Merge(page1day1, page1day1) == either one stat, or error if different counts
    // Some test checking which page rename takes precedence, both in correct ((prev,curr)) and flipped ((curr,prev)) ordering.
    //   Do this test by passing the same data but flipped current/previous.
    public class WikiPagesStatsStorageTests
    {
        [Theory]
        [ClassData(typeof(WikiPagesStatsStorageTestsData))]
        public void TestTheory(WikiPagesStatsTestPayload payload) { Verify(payload); }

        private static void Verify(WikiPagesStatsTestPayload data)
        {
            // Act - Split({foo, bar})
            (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth)? split =
                VerifySplitByMonth(data, data.SplitByMonthThrows ? typeof(ArgumentException) : null);

            // Act - Merge(foo, bar)
            WikiPageStats[]? merged = VerifyMerge(data, data.MergeThrows ? typeof(ArgumentException) : null);

            if (data.MergeThrows || data.SplitByMonthThrows) 
                return;

            // Act - Split(Merge(foo, bar)) == (foo, bar)
            var (previousMonthUnmerged, currentMonthUnmerged) =
                WikiPagesStatsStorage.SplitByMonth(merged!, data.Date);
            new JsonDiffAssertion(data.PreviousMonth, previousMonthUnmerged).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonthUnmerged).Assert();

            // Act - Merge(Split({foo, bar})) == Merge(foo, bar)
            var mergedSplit = WikiPagesStatsStorage.Merge(split!.Value.previousMonth, split!.Value.currentMonth);
            new JsonDiffAssertion(data.MergedPagesStats, mergedSplit).Assert();
        }

        private static (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth) VerifySplitByMonth(
            WikiPagesStatsTestPayload data)
        {
            // Act
            var (previousMonth, currentMonth) = WikiPagesStatsStorage.SplitByMonth(data.AllPagesStats, data.Date);
            new JsonDiffAssertion(data.PreviousMonth, previousMonth).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonth).Assert();
            return (previousMonth, currentMonth);
        }

        private static WikiPageStats[] VerifyMerge(WikiPagesStatsTestPayload data)
        {
            // Act
            var merged = WikiPagesStatsStorage.Merge(data.PreviousMonth, data.CurrentMonth);
            new JsonDiffAssertion(data.MergedPagesStats, merged).Assert();
            return merged;
        }

        private static (WikiPageStats[] previousMonth, WikiPageStats[] currentMonth)? VerifySplitByMonth(
            WikiPagesStatsTestPayload data, Type? excType) =>
            Verification.VerifyStruct(VerifySplitByMonth, data, excType);

        private static WikiPageStats[]? VerifyMerge(WikiPagesStatsTestPayload data, Type? excType) =>
            Verification.Verify<WikiPagesStatsTestPayload, WikiPageStats[]?>(VerifyMerge,
                data,
                excType);
    }
}