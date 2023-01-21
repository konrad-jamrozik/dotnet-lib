using System.Text.Json.Serialization;

namespace UfoGame.ViewModel;

public class ModalsState
{
    [JsonInclude] public bool IntroModalShown;

    public ModalsState()
        => Reset();

    public void Reset()
    {
        IntroModalShown = false;
    }
}