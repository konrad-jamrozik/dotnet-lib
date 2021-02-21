using System;
using System.Linq;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Tests.Json;
using Xunit;

namespace Wikitools.Tests
{
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

        private static void Verify(WikiPagesStatsTestData data)
        {
            VerifySplitInvariants(data);
            VerifyMergeInvariants(data);
        }

        private static void VerifySplitInvariants(WikiPagesStatsTestData data)
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
                var mergedSplit = ValidWikiPagesStats.Merge(split.previousMonth, split.currentMonth);
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

        private static void VerifyMergeInvariants(WikiPagesStatsTestData data)
        {
            // Act - Merge(prev, curr)
            ValidWikiPagesStats merged = VerifyMerge(data);

            // Act - Merge(Merge(prev, curr), Merge(prev, curr)) == Merge(prev, curr)
            var mergedTwice = ValidWikiPagesStats.Merge(merged!, merged!);
            new JsonDiffAssertion(merged!, mergedTwice).Assert();
        }

        private static void VerifySplitByMonth(
            WikiPagesStatsTestData data, Type? excType) =>
            Verification.VerifyStruct(VerifySplitByMonth, data, excType);

        private static (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth) VerifySplitByMonth(
            WikiPagesStatsTestData data)
        {
            // Act
            var (previousMonth, currentMonth) = ValidWikiPagesStats.SplitByMonth(data.AllPagesStats, data.Date);
            new JsonDiffAssertion(data.PreviousMonthWithCurrentMonthPaths, previousMonth).Assert();
            new JsonDiffAssertion(data.CurrentMonth, currentMonth).Assert();
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