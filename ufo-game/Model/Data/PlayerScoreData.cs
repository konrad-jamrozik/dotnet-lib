using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class PlayerScoreData : IPersistable, IResettable
{
    [JsonInclude] public int Value;

    public PlayerScoreData()
        => Reset();

    public void Reset()
    {
        Value = 100;
    }
}