using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public class GameState
{
    public readonly List<IPersistable> Persistables;
    private readonly List<IResettable> _resettables;

    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly GameStateStorage _storage;

    public GameState(
        GameStateStorage storage,
        ViewStateRefresh viewStateRefresh,
        IEnumerable<IPersistable> persistables,
        IEnumerable<IResettable> resettables)
    {
        Persistables = persistables.ToList();
        _resettables = resettables.ToList();
        _storage = storage;
        _viewStateRefresh = viewStateRefresh;
    }

    public void Persist()
        => _storage.Persist(this);

    public void Reset()
    {
        _resettables.ForEach(resettable => resettable.Reset());
        _storage.Clear();
        _viewStateRefresh.Trigger();
    }
}