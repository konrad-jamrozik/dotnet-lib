namespace UfoGame.ViewModel;

public interface IPlayerActionOnRangeInput
{
    void Act();
    string ActLabel();
    bool CanAct();
    bool CanAct(int value);
    int Input { get; set; }
    void IncrementInput();
    void DecrementInput();
    int InputMax();
    int InputMin();

    bool CanActOnIncrementedInput => CanAct(Input + 1);
    bool CanActOnDecrementedInput => CanAct(Input - 1);
}