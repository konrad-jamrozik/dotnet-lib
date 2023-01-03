namespace UfoGame.ViewModel;

class NoOpPlayerActionOnRangeInput : IPlayerActionOnRangeInput
{
    public void Act() { }

    public bool CanAct() => false;

    public bool CanAct(int value) => false;

    public int Input { get; set; }

    public void IncrementInput() { }

    public void DecrementInput() { }

    public int InputMax() => 0;

    public int InputMin() => 0;
}