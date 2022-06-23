namespace OxceTests;

public record CommendationBonuses
{
    public static CommendationBonuses Build(
        Commendations commendations,
        SoldierBonuses soldierBonuses)
    {
        return new CommendationBonuses();
    }

    public Soldier AddToSoldier(Soldier soldier)
    {
        var soldierWithBonuses = soldier with { CurrentMana = soldier.CurrentMana + 0 };
        return soldierWithBonuses;
    }
}