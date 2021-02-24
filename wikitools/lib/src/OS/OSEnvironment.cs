using System;

namespace Wikitools.Lib.OS
{
    public class OSEnvironment : IOSEnvironment
    {
        public string? Value(string varName) => Environment.GetEnvironmentVariable(varName);
    }
}