using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PlayerScoreData
{
    [JsonInclude] public int Value;

    public PlayerScoreData()
        => Reset();

    public void Reset()
    {
        Value = 1000;
    }
}