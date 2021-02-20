using System;

namespace Wikitools.Lib.Contracts
{
    public static class Contract
    {
        public static void Assert(int subject, string subjectName, Range range, string upperBoundReason)
        {
            if (!(subject >= range.Start.Value))
                throw new InvariantException(
                    $"{subjectName}: Expected value >= {range.Start.Value}; value = {subject}");
            if (!(subject <= range.End.Value))
                throw new InvariantException(
                    $"{subjectName}: Expected value <= {range.End.Value}. value = {subject}. " +
                    $"Reason: {upperBoundReason}");
        }
    }
}