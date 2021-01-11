using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib;
using Wikitools.Lib.Git;
using Wikitools.Lib.OS;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.Tables;

namespace Wikitools
{
    public class GitAuthorsStatsReportWriteOperation
    {
        private readonly MarkdownTable _table;

        public GitAuthorsStatsReportWriteOperation(
            ITimeline timeline, 
            IOperatingSystem os,
            string gitRepoDirPath,
            string gitExecutablePath,
            int logDays)
        {
            var repo = new GitRepository
            (
                new GitBashShell(os, gitExecutablePath), 
                gitRepoDirPath
            );
            var gitLog = new GitLog(repo, logDays);
            var report = new GitAuthorsStatsReport(timeline, gitLog, logDays);
            _table = new MarkdownTable(report);
        }

        public Task ExecuteAsync(TextWriter writer) 
            => _table.WriteAsync(writer);
    }
}