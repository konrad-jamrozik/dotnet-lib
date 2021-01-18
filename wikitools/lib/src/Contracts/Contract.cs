using System;

namespace Wikitools.Lib.Contracts
{
    public record Contract(int Subject, string SubjectName, Range Range)
    {
        public void Assert()
        {
            if (!(Subject >= Range.Start.Value))
                throw new ArgumentOutOfRangeException(SubjectName,
                    $"Expected value >= {Range.Start.Value}; value = {Subject}");
            if (!(Subject <= Range.End.Value))
                throw new ArgumentOutOfRangeException(SubjectName,
                    $"Expected value <= {Range.End.Value} as this is the ADO API limit; value = {Subject}");
        }
    }
}