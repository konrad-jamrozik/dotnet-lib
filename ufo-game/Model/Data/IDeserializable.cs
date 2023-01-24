namespace UfoGame.Model.Data;

/// <summary>
/// A type implementing this interface will be read, using reflection,
/// from persistent store by
///
///   UfoGame.Infra.PersistedGameStateReader.ReadOrReset()
///
/// and added to service collection upon app initialization.
/// </summary>
public interface IDeserializable
{
}