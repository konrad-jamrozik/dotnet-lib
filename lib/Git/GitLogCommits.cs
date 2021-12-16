using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wikitools.Lib.Contracts;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.Git
{
    public record GitLogCommits : IEnumerable<GitLogCommit>
    {
        private readonly IEnumerable<GitLogCommit> _commits;

        public readonly DaySpan DaySpan;

        public GitLogCommits(GitLogCommits commits, DaySpan daySpan)
        {
            Contract.Assert(daySpan.IsSubsetOf(commits.DaySpan));
            _commits = commits.Where(commit => daySpan.Contains(commit.Date));
            DaySpan = daySpan;
        }

        public IEnumerator<GitLogCommit> GetEnumerator()
            => _commits.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}