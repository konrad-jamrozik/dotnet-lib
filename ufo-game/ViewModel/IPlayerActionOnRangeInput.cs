namespace UfoGame.ViewModel;

public interface IPlayerActionOnRangeInput
{
    void Act();
    string ActLabel();
    bool CanAct();
    bool CanAct(int offset);
    int Input { get; set; }
    void IncrementInput();
    void DecrementInput();
    int InputMax();
    int InputMin();

    bool CanSetRangeInput => CanDecrementInput || CanIncrementInput;
    bool CanIncrementInput => CanActOnIncrementedInput;
    bool CanDecrementInput => CanActOnDecrementedInput;

    bool CanActOnIncrementedInput => CanAct(offset: 1);
    bool CanActOnDecrementedInput => CanAct(offset: -1);
}