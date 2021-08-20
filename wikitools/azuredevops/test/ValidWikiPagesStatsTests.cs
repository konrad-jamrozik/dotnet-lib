using System.Linq;
using Wikitools.Lib.Primitives;
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
            // Act - Split({foo, bar})
            var split = VerifySplitByMonth(
                target: data.AllPagesStats,
                previousMonthExpectation: 
                    data.AllPagesStats.StatsRangeIsWithinOneMonth() 
                        ? null 
                        : data.PreviousMonthWithCurrentMonthPaths,
                currentMonthExpectation: 
                    data.AllPagesStats.StatsRangeIsWithinOneMonth() 
                        ? data.PreviousMonthWithCurrentMonthPaths 
                        : data.CurrentMonthAfterSplit);

            // Act - Split(Split({prev, curr})[curr]) == curr
            VerifySplitByMonth(
                target: split.currentMonth,
                previousMonthExpectation: null,
                currentMonthExpectation: split.currentMonth);

            if (split.previousMonth != null)
            {
                // Act - Split(Split({prev, curr})[prev]) == prev
                VerifySplitByMonth(
                    target: split.previousMonth, 
                    previousMonthExpectation: null, 
                    currentMonthExpectation: split.previousMonth);

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
                    VerifySplitByMonth(mergedSplit, data.PreviousMonthAfterSplit,
                        data.CurrentMonthAfterSplit);
                }
            }
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

        private static (ValidWikiPagesStats? previousMonth, ValidWikiPagesStats currentMonth) VerifySplitByMonth(
            ValidWikiPagesStats target,
            ValidWikiPagesStats? previousMonthExpectation,
            ValidWikiPagesStats? currentMonthExpectation)
        {
            // Act
            var (previousMonth, currentMonth) = target.SplitIntoTwoMonths();
            
            AssertMonth(previousMonthExpectation, previousMonth);
            AssertMonth(currentMonthExpectation,  currentMonth);

            return (previousMonth, currentMonth);

            void AssertMonth(ValidWikiPagesStats? expectation, ValidWikiPagesStats? actual)
            {
                if (expectation != null)
                    new JsonDiffAssertion(expectation, actual!).Assert();
                else
                    Assert.Null(actual);
            }
        }

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