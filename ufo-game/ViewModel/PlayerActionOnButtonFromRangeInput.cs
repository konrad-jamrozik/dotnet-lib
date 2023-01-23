namespace UfoGame.ViewModel;

public class PlayerActionOnButtonFromRangeInput : IPlayerActionOnButton
{
    private readonly IPlayerActionOnRangeInput _playerActionOnRangeInput;

    public PlayerActionOnButtonFromRangeInput(IPlayerActionOnRangeInput playerActionOnRangeInput)
        => _playerActionOnRangeInput = playerActionOnRangeInput;

    public string ActLabel()
        => _playerActionOnRangeInput.ActLabel();

    public bool CanAct()
        => _playerActionOnRangeInput.CanAct();

    public void Act()
        => _playerActionOnRangeInput.Act();
}