namespace UfoGameLib.Model;

public class Missions : List<Mission>, ICloneable
{
    public object Clone()
    {
        var clone = new Missions();
        clone.AddRange(this.Select(mission => mission with { }));
        return clone;
    }
}