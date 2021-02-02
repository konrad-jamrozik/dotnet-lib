using System;
using Wikitools.Lib.Json;

namespace Wikitools.Lib.Tests.Json
{
    public record JsonDiffAssertion(JsonDiff Diff)
    {
        public JsonDiffAssertion(object baseline, object target) : this(new JsonDiff(baseline, target)) { }

        public void Assert()
        {
            Xunit.Assert.True(Diff.IsEmpty,
                $"The expected baseline is different than actual target. Diff:{Environment.NewLine}{Diff}");
        }
    }
}