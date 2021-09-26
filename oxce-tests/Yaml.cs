using System;
using System.Collections.Generic;

namespace OxceTests
{
    public class Yaml
    {
        private readonly string[] _lines;

        public Yaml(string[] lines)
        {
            _lines = lines;
        }

        public IEnumerable<Yaml> Sequence(string key)
        {
            // kja curr work
            throw new NotImplementedException();
        }

        public string Scalar(string key)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, int> MappingOfInts(string key)
        {
            throw new NotImplementedException();
        }
    }
}