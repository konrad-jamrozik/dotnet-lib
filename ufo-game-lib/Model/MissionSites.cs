namespace UfoGameLib.Model;

public class MissionSites : List<MissionSite>, ICloneable
{
    public object Clone()
    {
        var clone = new MissionSites();
        clone.AddRange(this.Select(site => site with { }));
        return clone;
    }
}