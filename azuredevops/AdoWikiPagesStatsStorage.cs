using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Storage;

namespace Wikitools.AzureDevOps;

/// <summary>
/// Represents a MonthlyJsonFilesStorage that stores stats data originating from IAdoWiki.
/// i.e. the data of type ValidWikiPagesStats.
///
/// For a class abstracting IAdoWiki backed by this storage, see AdoWikiWithStorage.
/// </summary>
public record AdoWikiPagesStatsStorage(MonthlyJsonFilesStorage Storage)
{
    public async Task<AdoWikiPagesStatsStorage> Update(ValidWikiPagesStats wikiPagesStats)
    {
        var (previousMonthStats, currentMonthStats) = wikiPagesStats.SplitIntoTwoMonths();
        if (previousMonthStats != null)
            await MergeIntoStoredMonthStats(previousMonthStats);
        await MergeIntoStoredMonthStats(currentMonthStats);

        return this;
    }

    public async Task<AdoWikiPagesStatsStorage> ReplaceWith(ValidWikiPagesStats stats)
    {
        IEnumerable<ValidWikiPagesStatsForMonth> statsByMonth = stats.SplitByMonth();

        var writeTasks = statsByMonth
            .Select(async statsForMonth
                => await Storage.With<IEnumerable<WikiPageStats>>(
                    statsForMonth.Month, _ => statsForMonth));

        await Task.WhenAll(writeTasks);

        return this;
    }

    public ValidWikiPagesStats PagesStats(DaySpan daySpan)
    {
        IEnumerable<ValidWikiPagesStatsForMonth> statsByMonth = DateMonth
            .MonthSpan(daySpan)
            .Select(month =>
            {
                var pageStats = Storage.Read<IEnumerable<WikiPageStats>>(month);
                return new ValidWikiPagesStatsForMonth(pageStats, month);
            });

        return ValidWikiPagesStats.Merge(statsByMonth).Trim(daySpan);
    }

    /// <summary>
    /// Merge 'stats', which pertain to given month, with the stored stats
    /// for that month, and store the result, replacing the previous
    /// stored stats for given month with the merged stats.
    ///
    /// Assumptions & preconditions:
    /// (1) the stored stats for given month can have stats for an earlier day
    /// than the earliest day present in 'stats'.
    /// (2) the stored stats for given month can not have stats for any day later
    /// than the latest day present in 'stats'.
    /// </summary>
    private async Task MergeIntoStoredMonthStats(ValidWikiPagesStatsForMonth stats)
        =>
            await Storage.With<IEnumerable<WikiPageStats>>(
                stats.Month,
                storedStats =>
                {
                    // The daySpan starts at the beginning of the month instead of stats.DaySpan.StartDay,
                    // to ensure that (1) holds.
                    // The daySpan ends at stats.DaySpan.EndDay, to ensure (2) holds.
                    var daySpan = new DaySpan(stats.Month.FirstDay, stats.DaySpan.EndDay);
                    var validStoredStats = new ValidWikiPagesStatsForMonth(storedStats, daySpan);
                    return new ValidWikiPagesStatsForMonth(validStoredStats.Merge(stats), daySpan);
                });
}