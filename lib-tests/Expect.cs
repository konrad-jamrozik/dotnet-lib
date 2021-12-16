using System;
using Xunit;
using Xunit.Sdk;

namespace Wikitools.Lib.Tests;

public static class Expect
{
    public static void Throws<TData, TReturn>(Func<TData, TReturn> target, TData data, Type excType) 
    {
        try
        {
            target(data);
        }
        catch (Exception e) when (e is not XunitException)
        {
            if (excType.IsInstanceOfType(e))
            {
                return;
            }

            Assert.False(true, e.Message + Environment.NewLine + e.StackTrace);
        }

        Assert.False(true, $"Expected exception of type {excType}");
    }
}