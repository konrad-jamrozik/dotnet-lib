using System;
using System.Collections.Generic;

namespace OxceTests
{
    // Partial parser for https://yaml.org/spec/1.2.2/
    public class Yaml
    {
        private readonly string[] _lines;

        public Yaml(string[] lines)
        {
            _lines = lines;
        }

        public IEnumerable<Yaml> Mapping(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Yaml> BlockSequence(string key)
        {
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