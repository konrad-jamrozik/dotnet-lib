namespace UfoGame.Infra;

public class RandomGen
{
    public readonly Random Random;

    public RandomGen()
    {
        int seed = new Random().Next();
        Console.WriteLine("RandomGen seed: " + seed);
        Random = new Random(seed);
    }
}