using System.Text.Json.Serialization;
using UfoGame.Model.Data;

namespace UfoGame.ViewModel;

public class ModalsState : IData, IResettable
{
    [JsonInclude] public bool IntroModalShown;

    public ModalsState()
        => Reset();

    public void Reset()
    {
        IntroModalShown = false;
    }
}