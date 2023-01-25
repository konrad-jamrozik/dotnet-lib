using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public class GameState
{
    public readonly List<IDeserializable> Deserializables;
    public readonly List<IResettable> Resettables;

    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly GameStateStorage _storage;

    public GameState(
        GameStateStorage storage,
        ViewStateRefresh viewStateRefresh,
        IEnumerable<IDeserializable> deserializables,
        IEnumerable<IResettable> resettables)
    {
        _storage = storage;
        _viewStateRefresh = viewStateRefresh;
        Deserializables = deserializables.ToList();
        Resettables = resettables.ToList();
    }

    // kja rename IDeserializable to IPersistable.
    public void PersistGameState()
        => _storage.Persist(this);

    public void Reset()
    {
        Resettables.ForEach(resettable => resettable.Reset());

        _storage.Clear();

        _viewStateRefresh.Trigger();
    }
}