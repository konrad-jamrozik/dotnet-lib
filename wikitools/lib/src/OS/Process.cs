using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Wikitools.Lib.Primitives;

namespace Wikitools.Lib.OS
{
    public class Process : IProcess
    {
        private readonly AsyncLazy<List<string>> _stdOutLines;

        public Process(string executableFilePath, Dir workingDir, params string[] arguments)
        {
            _stdOutLines = new AsyncLazy<List<string>>(
                async () =>
                {
                    if (!workingDir.Exists())
                    {
                        throw new InvalidOperationException(
                            $"Working directory doesn't exist. Path: {workingDir.Value}");
                    }

                    // Reference:
                    // https://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results
                    // https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo(v=vs.110).aspx
                    // Start the child process.
                    var p = new System.Diagnostics.Process
                    {
                        StartInfo =
                        {
                            // Redirect the output stream of the child process.
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            FileName = executableFilePath,
                            Arguments = string.Join(" ", arguments),
                            WorkingDirectory = workingDir.Value
                        }
                    };

                    p.Start();

                    var output = new List<string>();
                    while (!p.StandardOutput.EndOfStream)
                        output.Add(p.StandardOutput.ReadLine());

                    await p.WaitForExitAsync();

                    return output;
                }
            );
        }

        public Task<List<string>> GetStdOutLines() => _stdOutLines.Value;
    }
}
