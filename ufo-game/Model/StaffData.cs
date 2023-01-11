using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class StaffData
{
    [JsonInclude] public int SoldierEffectiveness;

    [JsonInclude] public int SoldierSurvivability;

    [JsonInclude] public int SoldiersToHire;

    [JsonInclude] public int CurrentSoldiers;

    public StaffData()
        => Reset();

    public void Reset()
    {
        SoldierEffectiveness = 100;
        SoldierSurvivability = 100;
        SoldiersToHire = 1;
        CurrentSoldiers = 0;
    }
}