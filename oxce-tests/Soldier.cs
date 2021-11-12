namespace OxceTests
{
    public record Soldier(
        string Type,
        string Name,
        int Missions,
        string BaseName,
        int Kills,
        int Rank,
        int MonthsService,
        int StatGainTotal,
        int CurrentPsiSkill) { }
}