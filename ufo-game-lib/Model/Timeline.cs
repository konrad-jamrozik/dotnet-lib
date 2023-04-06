namespace UfoGameLib.Model;

public record Timeline(int CurrentTurn)
{
    public int CurrentTurn { get; set; } = CurrentTurn;
}