using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrep
{
    [JsonInclude]
    public readonly MissionPrepData Data;


    public MissionPrep(MissionPrepData data)
    {
        Data = data;
    }
}