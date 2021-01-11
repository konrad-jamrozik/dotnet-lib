using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wikitools.Lib.Tables
{
    public interface ITabularData
    {
        public Task<string> GetDescription();
        public List<object> HeaderRow { get; }
        public Task<List<List<object>>> GetRows();
    }
}