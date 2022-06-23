namespace OxceTests;

public record Commendation(string Type, string[] SoldierBonusTypes)
{
    public static Commendation FromCommendationYaml(YamlMapping yaml)
    {
        var type = yaml.ParseString("type");
        var soldierBonusTypes = new YamlFlowSequence(yaml.Lines("soldierBonusTypes")).ToArray();

        return new Commendation(type, soldierBonusTypes);
    }
}