using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Oxce.Configs;
using Wikitools.Lib.Configuration;
using Wikitools.Lib.OS;

namespace OxceTests;

public class OxceTools
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task ProcessSaveFile()
    {
        await ParameterizedProcessSaveFile();
    }

    [Test]
    [Ignore("Input Save file deleted")]
    public async Task ProcessSaveFileOld()
    {
        await ParameterizedProcessSaveFile(
            "ufo landed.sav",
            "soldiers - ufo landed.csv",
            "item counts - ufo landed.csv");
    }

    [Test]
    public async Task ProcessSaveFileNew()
    {
        await ParameterizedProcessSaveFile(
            "ufo landed fixed reloaded.sav",
            "soldiers - ufo landed fixed reloaded.csv",
            "item counts - ufo landed fixed reloaded.csv");
    }

    private async Task ParameterizedProcessSaveFile(
        string saveFileName = null,
        string soldiersOutputFileName = null,
        string itemCountsOutputFileName = null)
    {
        var fs = new FileSystem();
        var cfg = new Configuration(fs).Load<IOxceCfg>(
            configProjectName: "oxce-configs",
            loadedTypeNamespace: "Oxce.Configs");

        var commendations = ReadCommendations(fs, cfg);
        var soldierBonuses = ReadSoldierBonuses(fs, cfg);
        var commendationBonuses = CommendationBonuses.Build(commendations, soldierBonuses);

        var bases = ReadBasesFromSaveFile(fs, cfg, saveFileName);

        await bases.WriteSoldiers(fs, SoldiersOutputPath(fs, cfg, soldiersOutputFileName), commendationBonuses);
        await bases.WriteItemCounts(fs, ItemCountsOutputPath(fs, cfg, itemCountsOutputFileName));
    }

    private static Commendations ReadCommendations(IFileSystem fs, IOxceCfg cfg)
    {
        var commendationsFilePath = cfg.CommendationsFilePath(fs);
        Console.Out.WriteLine("Reading " + commendationsFilePath);
        var commendationsYamlMapping =
            new YamlMapping(fs.ReadAllLines(commendationsFilePath));
        var commendations = Commendations.FromRulesetFileYaml(commendationsYamlMapping);
        return commendations;
    }

    private static SoldierBonuses ReadSoldierBonuses(IFileSystem fs, IOxceCfg cfg)
    {
        var soldierBonusesFilePath = cfg.SoldierBonusesFilePath(fs);
        Console.Out.WriteLine("Reading " + soldierBonusesFilePath);
        var soldierBonusesYamlMapping =
            new YamlMapping(fs.ReadAllLines(soldierBonusesFilePath));
        var soldierBonuses = SoldierBonuses.FromRulesetFileYaml(soldierBonusesYamlMapping);
        return soldierBonuses;
    }

    private static Bases ReadBasesFromSaveFile(IFileSystem fs, IOxceCfg cfg, string fileName)
    {
        var filePath = fileName != null
            ? fs.JoinPath(cfg.SaveDir(), fileName)
            : cfg.SaveFilePath(fs);
        Console.Out.WriteLine("Reading " + filePath);
        var saveFileYamlMapping = new YamlMapping(ParsedLines.FromFile(fs, filePath));
        var bases = Bases.FromSaveFileYaml(saveFileYamlMapping);
        return bases;
    }

    private static string ItemCountsOutputPath(
        IFileSystem fs,
        IOxceCfg cfg,
        string fileName)
        => fileName != null
            ? fs.JoinPath(cfg.OutputDir(), fileName)
            : cfg.ItemCountsOutputPath(fs);

    private static string SoldiersOutputPath(
        IFileSystem fs,
        IOxceCfg cfg,
        string fileName)
        => fileName != null
            ? fs.JoinPath(cfg.OutputDir(), fileName)
            : cfg.SoldiersOutputPath(fs);
}