using Wikitools.Lib.Configuration;
using Wikitools.Lib.OS;

namespace Oxce.Configs;

public interface IOxceCfg : IConfiguration
{
    public string SaveDir();
    public string SaveFileName();

    public string RulesetDir();
    public string SoldierBonusesFileName();
    public string CommendationsFileName();

    public string OutputDir();
    public string SoldiersOutputFileName();
    public string ItemCountsOutputFileName();

    public string SaveFilePath(IFileSystem fs) => fs.JoinPath(SaveDir(), SaveFileName());

    public string SoldierBonusesFilePath(IFileSystem fs) => fs.JoinPath(RulesetDir(), SoldierBonusesFileName());

    public string CommendationsFilePath(IFileSystem fs) => fs.JoinPath(RulesetDir(), CommendationsFileName());

    public string SoldiersOutputPath(IFileSystem fs) => fs.JoinPath(OutputDir(), SoldiersOutputFileName());

    public string ItemCountsOutputPath(IFileSystem fs) => fs.JoinPath(OutputDir(), ItemCountsOutputFileName());
}