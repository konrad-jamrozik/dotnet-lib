using System;
using System.Linq;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;
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
        [Fact] public void PageStatsPagesMissing()           => Verify(Data.PageStatsPagesMissing);
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
                VerifySplitByMonth(split.previousMonth, data.Date, split.previousMonth, null);

                // Act - Split(Split({prev, curr})[curr]) == curr
                VerifySplitByMonth(split.currentMonth, data.Date, null, split.currentMonth);

                // Act - Merge(Split({prev, curr})) == Merge(prev, curr)
                var mergedSplit = VerifyMerge(split.previousMonth, split.currentMonth, data.MergedPagesStats);

                if (!data.PageRenamePresent)
                {
                    // Act - Split({Merge[prev], Merge[curr]}) == {prev', curr'}
                    // where Merge = Merge(Split({prev, curr})) 
                    // where prev', curr' denote page stats for the corresponding months, but also
                    // having empty page entries for pages that were not yet existing (in case of prev month) or were
                    // since deleted (in case of curr month), instead of these pages being completely absent from the
                    // prev and curr data. For details why is that, please see the comment on data.CurrentMonthAfterSplit.
                    VerifySplitByMonth(mergedSplit, data.Date, data.PreviousMonthAfterSplit, data.CurrentMonthAfterSplit);
                }
            }
            else
                VerifySplitByMonthThrows(data, typeof(InvariantException));
        }

        private static void VerifyMergeInvariants(ValidWikiPagesStatsTestData data)
        {
            // Act - Merge(prev, curr)
            ValidWikiPagesStats merged = VerifyMerge(
                data.PreviousMonthToMerge,
                data.CurrentMonthToMerge,
                data.MergedPagesStats);
            
            // Act - Merge(Merge(prev, curr), Merge(prev, curr)) == Merge(prev, curr)
            VerifyMerge(merged, merged, merged);
        }

        private static (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth) VerifySplitByMonth(
            ValidWikiPagesStats target,
            DateTime date,
            ValidWikiPagesStats? previousMonthExpectation,
            ValidWikiPagesStats? currentMonthExpectation)
        {
            // Act
            var (previousMonth, currentMonth) = target.SplitByMonth(new DateMonth(date));
            
            AssertMonth(previousMonthExpectation, previousMonth);
            AssertMonth(currentMonthExpectation,  currentMonth);

            return (previousMonth, currentMonth);

            void AssertMonth(ValidWikiPagesStats? expectation, ValidWikiPagesStats actual)
            {
                if (expectation != null)
                    new JsonDiffAssertion(expectation, actual).Assert();
                else
                    Assert.DoesNotContain(actual, ps => ps.DayStats.Any());
            }
        }

        private static (ValidWikiPagesStats previousMonth, ValidWikiPagesStats currentMonth) VerifySplitByMonth(
                ValidWikiPagesStatsTestData data) =>
            VerifySplitByMonth(
                data.AllPagesStats,
                data.Date,
                data.PreviousMonthWithCurrentMonthPaths,
                data.CurrentMonthAfterSplit);

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