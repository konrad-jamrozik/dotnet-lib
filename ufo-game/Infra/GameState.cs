using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public class GameState
{
    public readonly List<IDeserializable> Deserializables;
    private readonly List<IResettable> _resettables;

    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly GameStateStorage _storage;

    public GameState(
        GameStateStorage storage,
        ViewStateRefresh viewStateRefresh,
        IEnumerable<IDeserializable> deserializables,
        IEnumerable<IResettable> resettables)
    {
        Deserializables = deserializables.ToList();
        _resettables = resettables.ToList();
        _storage = storage;
        _viewStateRefresh = viewStateRefresh;
    }

    // kja rename IDeserializable to IPersistable.
    public void Persist()
        => _storage.Persist(this);

    public void Reset()
    {
        _resettables.ForEach(resettable => resettable.Reset());
        _storage.Clear();
        _viewStateRefresh.Trigger();
    }
}