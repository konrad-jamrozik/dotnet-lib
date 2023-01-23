namespace UfoGame.ViewModel;

public interface IPlayerActionOnButton
{
    string ActLabel();
    bool CanAct();
    void Act();
}