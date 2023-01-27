namespace UfoGame.Model.Data;

/// <summary>
/// All implementers of this interface will have their Reset method called
/// as a result of invocation of:
///
///   UfoGame.Infra.GameState.Reset
/// 
/// </summary>

public interface IResettable
{
    public void Reset();
}