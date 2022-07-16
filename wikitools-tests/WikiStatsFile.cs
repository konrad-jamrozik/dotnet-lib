using System;
using System.Text.RegularExpressions;
using Wikitools.AzureDevOps;
using Wikitools.Lib.Json;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;

namespace Wikitools.Tests;

internal class WikiStatsFile
{
    private const string DateFormatString = "yyyy_MM_dd";

    internal static string Regex => @"wiki_stats_(\d\d\d\d_\d\d_\d\d)_(\d+)days.json";

    internal string Name => $"wiki_stats_{_dateTime.ToString(DateFormatString)}_{_pageViewsForDays}days.json";

    private readonly File _file;
    private readonly DateTime _dateTime;
    private readonly int _pageViewsForDays;

    public WikiStatsFile(File file)
    {
        _file = file;
        var (dateTime, pageViewsForDays) = ParseFromFilePath(file.Path);
        _dateTime = dateTime;
        _pageViewsForDays = pageViewsForDays;
    }

    public WikiStatsFile(Dir dir, DateTime dateTime, int pageViewsForDays)
    {
        _dateTime = dateTime;
        _pageViewsForDays = pageViewsForDays;
        _file = new File(dir, Name);
    }

    public ValidWikiPagesStats Stats => DeserializeStats(_file.FileSystem, (_file.Path, DaySpan()));

    private static (DateTime dateTime, int pageViewsForDays) ParseFromFilePath(string path)
    {
        Match match = new Regex(Regex).Match(path);
        var matchGroup = match.Groups[1];
        var dateTime = DateTime.ParseExact(matchGroup.Value, DateFormatString, null);
        var pageViewsForDays = int.Parse(match.Groups[2].Value);
        return (dateTime, pageViewsForDays);
    }

    private static ValidWikiPagesStats DeserializeStats(
        IFileSystem fs,
        (string statsPath, DaySpan daySpan) statsData)
        => new ValidWikiPagesStats(
            fs.ReadAllText(statsData.statsPath).FromJsonTo<WikiPageStats[]>(),
            statsData.daySpan);

    private DaySpan DaySpan()
    {
        var (dateTime, pageViewsForDays) = ParseFromFilePath(_file.Path);
        var daySpan = pageViewsForDays.AsDaySpanUntil(new DateDay(dateTime));

        return daySpan;
    }
}