using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wikitools.Lib.Git;

namespace Wikitools
{
    public class TabularData2 : IFormattableAsMarkdown
    {
        public TabularData2(List<string> headerRow, Task<List<List<object>>> rows)
        {
            throw new NotImplementedException();
        }

        public TabularData2(Task<(List<string> headerRow, List<List<object>> rows)> rows)
        {
            throw new NotImplementedException();
        }
    }
}