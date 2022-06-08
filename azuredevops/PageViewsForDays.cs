using System;
using Wikitools.Lib.Contracts;

namespace Wikitools.AzureDevOps;

/// <summary>
/// Represents the "pageViewsForDays" request body integer when obtaining data from ADO wiki
/// See Wikitools.AzureDevOps.IWikiHttpClient implementations for details on the ADO API calls.
/// </summary>
public record PageViewsForDays()
{
    // If 0, the call to ADO wiki API still succeeds, but all returned WikiPageDetail will have null ViewStats.
    // Confirmed empirically as of 3/27/2021.
    public const int Min = 1;

    // Max value supported by ADO API.
    // Confirmed empirically as of 3/27/2021.
    public const int Max = 30;

    public PageViewsForDays(int? days) : this()
    {
        Days = days ?? 0;
        AssertWithinAdoApiLimit();
    }

    public int Days { get; }

    public override string ToString() => $"{Days}";

    private void AssertWithinAdoApiLimit()
        => Contract.Assert(
            Days,
            nameof(PageViewsForDays),
            new Range(Min, Max),
            upperBoundReason: "ADO API limit");
}