using System;
using Wikitools.Lib.Contracts;

namespace Wikitools.AzureDevOps;

public record PageViewsForDays()
{
    // Max value supported by https://docs.microsoft.com/en-us/rest/api/azure/devops/wiki/pages-batch/get?view=azure-devops-rest-6.1
    // Confirmed empirically as of 3/27/2021.
    public const int Max = 30;
    // If 0, the call to ADO wiki API still succeeds, but all returned WikiPageDetail will have null ViewStats.
    // Confirmed empirically as of 3/27/2021.
    public const int Min = 1;

    public PageViewsForDays(int value) : this()
    {
        Contract.Assert(value >= Min);
        Value = value;
    }

    public void AssertPageViewsForDaysRange()
        => Contract.Assert(
            Value,
            nameof(PageViewsForDays),
            new Range(Min, Max),
            upperBoundReason: "ADO API limit");


    public static implicit operator PageViewsForDays(int value) => new PageViewsForDays(value);

    public int Value { get; }
}