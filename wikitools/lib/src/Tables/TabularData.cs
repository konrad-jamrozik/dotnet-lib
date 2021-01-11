using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.Lib.Tables
{
    public record TabularData(
        string Description, 
        List<object> HeaderRow, 
        List<List<object>> Rows) : ITabularData
    {
        public Task<string> GetDescription() => Task.FromResult(Description);
        public Task<List<List<object>>> GetRows() => Task.FromResult(Rows);
    }
}