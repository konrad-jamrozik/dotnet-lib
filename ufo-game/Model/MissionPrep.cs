namespace UfoGame.Model;

public class MissionPrep
{
    public int SoldiersToSend { get; set; }

    public void IncrementSoldiersToSend() => SoldiersToSend += 1;

    public void DecrementSoldiersToSend() => SoldiersToSend -= 1;
}