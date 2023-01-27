namespace UfoGame.Model.Data;

/// <summary>
/// Every type implementing this interface will be persisted, using reflection,
/// to a persistent store upon invocation of:
///
///   UfoGame.Infra.GameState.Persist
/// 
/// And conversely, every such type will be read, using reflection,
/// from persistent upon invocation of:
///
///   UfoGame.Infra.PersistedGameStateReader.ReadOrReset()
///
/// </summary>
public interface IPersistable
{
}