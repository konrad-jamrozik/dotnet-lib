namespace UfoGame.Model.Data;

/// <summary>
/// Every type implementing this interface will be persisted, using reflection,
/// to a store upon invocation of:
///
///   UfoGame.Infra.GameState.Persist
/// 
/// Conversely, every such type will be read, using reflection,
/// from the store upon invocation of:
///
///   UfoGame.Infra.PersistedGameStateReader.ReadOrReset
///
/// </summary>
public interface IPersistable
{
}