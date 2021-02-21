using System;
using System.Linq;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Tests;
using Wikitools.Lib.Tests.Json;
using Xunit;
using Data = Wikitools.AzureDevOps.Tests.ValidWikiPagesStatsTestDataFixture;

namespace Wikitools.AzureDevOps.Tests
{
    public class ValidWikiPagesStatsTests
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
        [Fact] public void PageStatsRenamedToNewPath()       => Verify(Data.PageStatsRenamedToNewPath);
        [Fact] public void PageStatsExchangedPaths()         => Verify(Data.PageStatsExchangedPaths);
        // @formatter:on

        private static void Verify(ValidWikiPagesStatsTestData data)
        {
            VerifySplitInvariants(data);
            VerifyMergeInvariants(data);
        }

        private static void VerifySplitInvariants(ValidWikiPagesStatsTestData data)
        {
            if (!data.SplitPreconditionsViolated)
            {
                // Act - Split({foo, bar})
                var split = VerifySplitByMonth(data);

                // Act - Split(Split({prev, curr})[prev]) == prev
                var (previousPreviousMonth, currentPreviousMonth) =
                    ValidWikiPagesStats.SplitByMonth(split.previousMonth, data.Date);
                new JsonDiffAssertion(split.previousMonth.Value, previousPreviousMonth.Value).Assert();
                Assert.DoesNotContain(currentPreviousMonth.Value, ps => ps.DayStats.Any());

                // Act - Split(Split({prev, curr})[curr]) == curr
                var (previousCurrentMonth, currentCurrentMonth) =
                    ValidWikiPagesStats.SplitByMonth(split.currentMonth, data.Date);
                Assert.DoesNotContain(previousCurrentMonth.Value, ps => ps.DayStats.Any());
                new JsonDiffAssertion(split.currentMonth.Value, currentCurrentMonth.Value).Assert();

                // Act - Merge(Split({prev, curr})) == Merge(prev, curr)
                var mergedSplit = split.previousMonth.Merge(split.currentMonth);
                new JsonDiffAssertion(data.MergedPagesStats, mergedSplit).Assert();

                if (!data.PageRenamePresent)
                {
                    // Act - Split({Merge[prev], Merge[curr]}) == {prev, curr}
                    // where Merge = Merge(Split({prev, curr})) 
                    var (previousMonthPostMerge, currentMonthPostMerge) =
                        ValidWikiPagesStats.SplitByMonth(mergedSplit, data.Date);
                    new JsonDiffAssertion(data.PreviousMonth, previousMonthPostMerge).Assert();
                    new JsonDiffAssertion(data.CurrentMonth,  currentMonthPostMerge).Assert();
                }
            }
            else
                VerifySplitByMonth(data, typeof(InvariantException));
        }

        private static void VerifyMergeInvariants(ValidWikiPagesStatsTestData data)
        {
            // Act - Merge(prev, curr)
            ValidWikiPagesStats merged = VerifyMerge(data);

            // Act - Merge(Merge(prev, curr), Merge(prev, curr)) == Merge(prev, curr)
            var mergedTwice = merged!.Merge(merged!);
            new JsonDiffAssertion(merged!, mergedTwice).Assert();
        }

        private static void VerifySplitByMonth(
            ValidWikiPagesStatsTestData data, Type excType) => TestAssert.Throws(VerifySplitByMonth, data, excType);

        private static (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth) VerifySplitByMonth(
            ValidWikiPagesStatsTestData data)
        {
            // Act
            var (previousMonth, currentMonth) = ValidWikiPagesStats.SplitByMonth(data.AllPagesStats, data.Date);
            new JsonDiffAssertion(data.PreviousMonthWithCurrentMonthPaths, previousMonth).Assert();
            new JsonDiffAssertion(data.CurrentMonth, currentMonth).Assert();
            return (previousMonth, currentMonth);
        }

        private static ValidWikiPagesStats VerifyMerge(ValidWikiPagesStatsTestData data)
        {
            // Act
            var merged = data.PreviousMonth.Merge(data.CurrentMonth);
            new JsonDiffAssertion(data.MergedPagesStats, merged).Assert();
            return merged;
        }
    }
}