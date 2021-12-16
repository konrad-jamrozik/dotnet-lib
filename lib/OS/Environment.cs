namespace Wikitools.Lib.OS;

public class Environment : IEnvironment
{
    public string? Value(string varName) => System.Environment.GetEnvironmentVariable(varName);
}