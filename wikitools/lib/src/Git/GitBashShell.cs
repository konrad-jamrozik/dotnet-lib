using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;
using Wikitools.Lib.OS;

namespace Wikitools.Lib.Git
{
    public class GitBashShell : IShell
    {
        private readonly IOperatingSystem _os;
        private readonly string _gitExecutablePath;

        public GitBashShell(IOperatingSystem os, string gitExecutablePath)
        {
            _os = os;
            _gitExecutablePath = gitExecutablePath;
        }

        public Task<List<string>> GetStdOutLines(string workingDirPath, string[] arguments)
        {
            var executableFilePath = _gitExecutablePath.Replace(@"\", @"\\");

            // Reference:
            // https://stackoverflow.com/questions/17302977/how-to-launch-git-bash-from-dos-command-line
            // https://superuser.com/questions/1104567/how-can-i-find-out-the-command-line-options-for-git-bash-exe
            string[] processArguments =
            {
                "--login", 
                "-c", 
                new QuotedString(string.Join(" ", arguments)).Value
            };
            
            IProcess process = _os.Process(executableFilePath, workingDirPath, processArguments);

            return process.GetStdOutLines();
        }
    }
}
