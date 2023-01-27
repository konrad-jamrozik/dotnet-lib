namespace UfoGame.Model;

/// <summary>
/// All implementers of this interface will have their AdvanceTime method called
/// as a result of invocation of:
///
///   UfoGame.Model.Timeline.AdvanceTime
/// 
/// </summary>
public interface ITemporal
{
    public void AdvanceTime();
}