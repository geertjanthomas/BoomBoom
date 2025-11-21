using System.Threading;
using System.Threading.Tasks;

namespace BoomBoom.Services;

public class GameService
{
    private static GameService? _instance;
    public static GameService Instance => _instance ??= new GameService();

    private Game? _game;
    private SynchronizationContext? _uiContext;

    public event Action<GameConfiguration>? OnStartNewGame;

    public void RegisterGame(Game game, SynchronizationContext? uiContext)
    {
        _game = game;
        if (uiContext != null)
        {
            _uiContext = uiContext;
        }
    }

    public Game? GetGame() => _game;

    public void RequestNewGame(GameConfiguration configuration)
    {
        if (_uiContext != null)
        {
            _uiContext.Post(_ => OnStartNewGame?.Invoke(configuration), null);
        }
        else
        {
            OnStartNewGame?.Invoke(configuration);
        }
    }

    public Task ExecuteOnUI(Action action)
    {
        var tcs = new TaskCompletionSource();
        if (_uiContext != null)
        {
            _uiContext.Post(_ =>
            {
                try
                {
                    action();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);
        }
        else
        {
            action();
            tcs.SetResult();
        }
        return tcs.Task;
    }
}
