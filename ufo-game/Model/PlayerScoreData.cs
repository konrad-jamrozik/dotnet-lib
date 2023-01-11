using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PlayerScoreData
{
    [JsonInclude]
    public int Value = 1000;
}