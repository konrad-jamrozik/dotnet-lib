using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
    // kja 1 remaining merge tests to add
    // Some test checking which page rename takes precedence, both in correct ((prev,curr)) and flipped ((curr,prev)) ordering.
    //   Do this test by passing the same data but flipped current/previous.
    public class ValidWikiPagesStatsTests // kja move to AzureDevOps project & namespace
    {
        // @formatter:off
        [Fact] public void PageStatsEmpty()                  => Verify(ValidWikiPagesStatsTestsData.PageStatsEmpty);
        [Fact] public void PageStatsYearWrap()               => Verify(ValidWikiPagesStatsTestsData.PageStatsYearWrap);
        [Fact] public void PageStatsBeforeYearWrap()         => Verify(ValidWikiPagesStatsTestsData.PageStatsBeforeYearWrap);
        [Fact] public void PageStatsPreviousMonthOnly()      => Verify(ValidWikiPagesStatsTestsData.PageStatsPreviousMonthOnly);
        [Fact] public void PageStats()                       => Verify(ValidWikiPagesStatsTestsData.PageStats);
        [Fact] public void PageStatsSameDay()                => Verify(ValidWikiPagesStatsTestsData.PageStatsSameDay);
        [Fact] public void PageStatsSamePreviousDay()        => Verify(ValidWikiPagesStatsTestsData.PageStatsSamePreviousDay);
        [Fact] public void PageStatsSameDayDifferentCounts() => Verify(ValidWikiPagesStatsTestsData.PageStatsSameDayDifferentCounts);
        [Fact] public void PageStatsSameMonth()              => Verify(ValidWikiPagesStatsTestsData.PageStatsSameMonth);
        [Fact] public void PageStatsRenamedToNewPath()       => Verify(ValidWikiPagesStatsTestsData.PageStatsRenamedToNewPath);
        [Fact] public void PageStatsExchangedPaths()         => Verify(ValidWikiPagesStatsTestsData.PageStatsExchangedPaths);
        // @formatter:on

        // kja 3 move these methods to ValidWikiPagesStatsTests
        private static void Verify(WikiPagesStatsTestData data)
        {
            VerifySplitLogic(data);
            VerifyMergeLogic(data);
        }

        private static void VerifySplitLogic(WikiPagesStatsTestData data)
        {
            // Act - Split({foo, bar})
            (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth)? split =
                VerifySplitByMonth(data, data.SplitPreconditionsViolated ? typeof(InvariantException) : null);

            if (!data.SplitPreconditionsViolated)
            {
                // Act - Split(Split({foo, bar})[prev]) == prev
                var (previousPreviousMonth, currentPreviousMonth) = ValidWikiPagesStats.SplitByMonth(split!.Value.previousMonth, data.Date);
                new JsonDiffAssertion(split!.Value.previousMonth.Value, previousPreviousMonth.Value).Assert();
                Assert.DoesNotContain(currentPreviousMonth.Value, ps => ps.DayStats.Any());

                // Act - Split(Split({foo, bar})[curr]) == curr
                var (previousCurrentMonth, currentCurrentMonth) = ValidWikiPagesStats.SplitByMonth(split!.Value.currentMonth, data.Date);
                Assert.DoesNotContain(previousCurrentMonth.Value, ps => ps.DayStats.Any());
                new JsonDiffAssertion(split!.Value.currentMonth.Value, currentCurrentMonth.Value).Assert();

                // Act - Merge(Split({foo, bar})) == Merge(prev, curr)
                var mergedSplit = ValidWikiPagesStats.Merge(split!.Value.previousMonth, split!.Value.currentMonth);
                new JsonDiffAssertion(data.MergedPagesStats, mergedSplit).Assert();
            }
        }

        private static void VerifyMergeLogic(WikiPagesStatsTestData data)
        {
            // Act - Merge(prev, curr)
            ValidWikiPagesStats? merged = VerifyMerge(data, null); // kja simplify

            // Act - Merge(Merge(prev, curr), Merge(prev, curr)) == Merge(prev, curr)
            var mergedTwice = ValidWikiPagesStats.Merge(merged!, merged!);
            new JsonDiffAssertion(merged!, mergedTwice).Assert();

            // The invariant verified below does not hold if page rename is present.
            // This is because Merge erases the previous name.
            if (!data.PageRenamePresent)
            {
                // Act - Split(Merge(foo, bar)) == {foo, bar}
                var (previousMonthPostMerge, currentMonthPostMerge) =
                    ValidWikiPagesStats.SplitByMonth(merged!, data.Date);
                // kja this fails for SameDay test because PreviousMonthAfterSplit fixture has February date in it, which is erased by the Split
                // The underlying problem here is that I am trying to reuse prev & curr months also for Merge, but that doesn't make sense,
                // because for Merge I need an overlap. I am trying to simulate this overlap by putting wrong dates in months, but that
                // screws up split logic.
                new JsonDiffAssertion(data.PreviousMonthAfterSplit, previousMonthPostMerge).Assert();
                new JsonDiffAssertion(data.CurrentMonth,            currentMonthPostMerge).Assert();
            }
        }

        private static (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth)? VerifySplitByMonth(
            WikiPagesStatsTestData data, Type? excType) =>
            Verification.VerifyStruct(VerifySplitByMonth, data, excType);

        private static ValidWikiPagesStats? VerifyMerge(WikiPagesStatsTestData data, Type? excType) =>
            Verification.Verify<WikiPagesStatsTestData, ValidWikiPagesStats?>(VerifyMerge,
                data,
                excType);

        private static (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth) VerifySplitByMonth(
            WikiPagesStatsTestData data)
        {
            // Act
            var (previousMonth, currentMonth) = ValidWikiPagesStats.SplitByMonth(data.AllPagesStats, data.Date);
            new JsonDiffAssertion(data.PreviousMonthAfterSplit, previousMonth).Assert();
            new JsonDiffAssertion(data.CurrentMonth,  currentMonth).Assert();
            return (previousMonth, currentMonth);
        }

        private static ValidWikiPagesStats VerifyMerge(WikiPagesStatsTestData data)
        {
            // Act
            var merged = ValidWikiPagesStats.Merge(data.PreviousMonth, data.CurrentMonth);
            new JsonDiffAssertion(data.MergedPagesStats, merged).Assert();
            return merged;
        }
    }
}