using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class StaffData
{
    [JsonInclude]
    public int SoldierEffectiveness = 100;

    [JsonInclude]
    public int SoldierSurvivability = 100;

    [JsonInclude]
    public int SoldiersToHire = 1;

    [JsonInclude]
    public int CurrentSoldiers = 0;
}