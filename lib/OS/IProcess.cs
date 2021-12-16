using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.Lib.OS;

public interface IProcess
{
    Task<List<string>> GetStdOutLines();
}