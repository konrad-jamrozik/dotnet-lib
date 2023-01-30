namespace UfoGame.Infra;

public class RandomGen
{
    public readonly Random Random;

    public RandomGen()
    {
        int seed = new Random().Next();
        Random = new Random(seed);
    }
}