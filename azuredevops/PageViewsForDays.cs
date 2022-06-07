﻿using System;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

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

    public PageViewsForDays(DaySpan daySpan) : this(daySpan.Count)
    {
    }

    public PageViewsForDays(int? value) : this(value ?? 0)
    {
    }

    public PageViewsForDays(int value) : this()
    {
        Contract.Assert(value >= Min);
        Value = value;
        // kja have this here instead of the Min check above, but note it makes a test fail:
        // Wikitools.AzureDevOps.Tests.AdoWikiWithStorageTests.DataFromStorageFromManyMonths
        //AssertPageViewsForDaysRange();
    }

    public int Value { get; }

    public int ValueWithinAdoApiLimit
    {
        get
        {
            AssertPageViewsForDaysRange();
            return Value;
        }
    }

    public static readonly PageViewsForDays Today = new PageViewsForDays(1);

    public static implicit operator PageViewsForDays(int value) => new PageViewsForDays(value);

    public PageViewsForDays MinWith(int? value) => new PageViewsForDays(Value.MinWith(value));

    public void AssertPageViewsForDaysRange() // kj2 rename: AssertWithinAdoApiLimit
        => Contract.Assert(
            Value,
            nameof(PageViewsForDays),
            new Range(Min, Max),
            upperBoundReason: "ADO API limit");

    public override string ToString() => $"{Value}";

    public DaySpan AsDaySpanUntil(DateDay endDay)
        => new DaySpan(endDay.AddDays(-Value+1), endDay);
}