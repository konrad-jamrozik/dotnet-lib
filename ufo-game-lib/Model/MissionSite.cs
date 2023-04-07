namespace UfoGameLib.Model;

public record MissionSite(int Id, bool IsActive)
{
    public bool IsActive { get; set; } = true;
}