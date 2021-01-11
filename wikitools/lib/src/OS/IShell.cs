using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.Lib.OS
{
    public interface IShell
    {
        Task<List<string>> GetStdOutLines(string workingDirPath, string[] arguments);
    }
}