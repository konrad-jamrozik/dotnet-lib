namespace UfoGameLib.Model;

public record Mission
{
    public MissionSite Site { get; set; }

    public Mission(MissionSite site)
    {
        Debug.Assert(site.IsActive);
        Site = site;
    }
}
