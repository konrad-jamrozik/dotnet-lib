namespace UfoGameLib.Model;

public class Agents : List<Agent>, ICloneable
{
    public object Clone()
    {
        var clone = new Agents();
        clone.AddRange(this.Select(agent => agent with { }));
        return clone;
    }
}