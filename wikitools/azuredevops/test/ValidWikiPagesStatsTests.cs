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
        [Fact] public void PageStatsInterleavingDayStats()   => Verify(Data.PageStatsInterleavingDayStats);
        [Fact] public void PageStatsRenamedToNewPath()       => Verify(Data.PageStatsRenamedToNewPath);
        [Fact] public void PageStatsExchangedPaths()         => Verify(Data.PageStatsExchangedPaths);
        // @formatter:on

        private static void Verify(ValidWikiPagesStatsTestData data)
        {
            VerifySplitByMonthInvariants(data);
            VerifyMergeInvariants(data);
        }

        private static void VerifySplitByMonthInvariants(ValidWikiPagesStatsTestData data)
        {
            if (!data.SplitPreconditionsViolated)
            {
                // Act - Split({foo, bar})
                var split = VerifySplitByMonth(data);

                // Act - Split(Split({prev, curr})[prev]) == prev
                var (previousPreviousMonth, currentPreviousMonth) =
                    split.previousMonth.SplitByMonth(data.Date);
                new JsonDiffAssertion(split.previousMonth, previousPreviousMonth).Assert();
                Assert.DoesNotContain(currentPreviousMonth, ps => ps.DayStats.Any());

                // Act - Split(Split({prev, curr})[curr]) == curr
                var (previousCurrentMonth, currentCurrentMonth) =
                    split.currentMonth.SplitByMonth(data.Date);
                Assert.DoesNotContain(previousCurrentMonth, ps => ps.DayStats.Any());
                new JsonDiffAssertion(split.currentMonth, currentCurrentMonth).Assert();

                // Act - Merge(Split({prev, curr})) == Merge(prev, curr)
                var mergedSplit = VerifyMerge(split.previousMonth, split.currentMonth, data.MergedPagesStats);

                if (!data.PageRenamePresent)
                {
                    // Act - Split({Merge[prev], Merge[curr]}) == {prev, curr}
                    // where Merge = Merge(Split({prev, curr})) 
                    VerifySplitByMonth(mergedSplit, data.Date, data.PreviousMonth, data.CurrentMonth);
                }
            }
            else
                VerifySplitByMonthThrows(data, typeof(InvariantException));
        }

        private static void VerifyMergeInvariants(ValidWikiPagesStatsTestData data)
        {
            // Act - Merge(prev, curr)
            ValidWikiPagesStats merged = VerifyMerge(data.PreviousMonth, data.CurrentMonth, data.MergedPagesStats);
            
            // Act - Merge(Merge(prev, curr), Merge(prev, curr)) == Merge(prev, curr)
            VerifyMerge(merged, merged, merged);
        }

        private static (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth) VerifySplitByMonth(
            ValidWikiPagesStats target,
            DateTime date,
            ValidWikiPagesStats previousMonthExpectation,
            ValidWikiPagesStats currentMonthExpectation)
        {
            // Act
            var (previousMonth, currentMonth) = target.SplitByMonth(date);
            new JsonDiffAssertion(previousMonthExpectation, previousMonth).Assert();
            new JsonDiffAssertion(currentMonthExpectation,  currentMonth).Assert();
            return (previousMonth, currentMonth);
        }

        private static (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth) VerifySplitByMonth(
                ValidWikiPagesStatsTestData data) =>
            VerifySplitByMonth(
                data.AllPagesStats,
                data.Date,
                data.PreviousMonthWithCurrentMonthPaths,
                data.CurrentMonth);

        private static void VerifySplitByMonthThrows(
            ValidWikiPagesStatsTestData data, Type excType) => Expect.Throws(VerifySplitByMonth, data, excType);

        private static ValidWikiPagesStats VerifyMerge(
            ValidWikiPagesStats target,
            ValidWikiPagesStats input,
            ValidWikiPagesStats expectation)
        {
            // Act
            var merged = target.Merge(input);
            new JsonDiffAssertion(expectation, merged).Assert();
            return merged;
        }
    }
}