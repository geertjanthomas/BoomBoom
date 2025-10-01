using System.Collections;
using System.Diagnostics;

namespace BoomBoom;

public class Game
{
    private readonly Stopwatch _stopwatch;

    public bool IsRunning { get; private set; }

    public bool IsPaused { get; private set; }

    public GameConfiguration Configuration { get; }

    public GameCell[,] Grid { get; }

    public TimeSpan GameTime => _stopwatch.Elapsed;

    public event EventHandler? Lose;

    public event EventHandler? Win;

    public event EventHandler? Click;

    private IEnumerable<GameCell> AllCells => ((IEnumerable)Grid).Cast<GameCell>();

    public Game(GameConfiguration configuration)
    {
        _stopwatch = new Stopwatch();
        Configuration = configuration;
        var now = DateTime.Now;
        var millisecond = now.Millisecond;
        var microsecond = now.Microsecond;
        var random = new Random(millisecond * microsecond);
        var intList = new List<int>();

        while (intList.Count < configuration.NumberOfBombs)
        {
            var num = random.Next(configuration.Height * configuration.Width);
            if (!intList.Contains(num))
            {
                intList.Add(num);
            }
        }

        Grid = new GameCell[configuration.Width, configuration.Height];
        for (var c = 0; c < configuration.Width; ++c)
        {
            for (var r = 0; r < configuration.Height; ++r)
            {
                var num = c * configuration.Height + r;
                Grid[c, r] = new GameCell()
                {
                    Column = c,
                    Row = r,
                    HasBomb = intList.Contains(num)
                };
            }
        }
        for (var c = 0; c < configuration.Width; ++c)
        {
            for (var r = 0; r < configuration.Height; ++r)
            {
                if (r > 0)
                {
                    Grid[c, r].Neighbours[Neighbour.Top] = Grid[c, r - 1];
                }
                if (r > 0 && c < configuration.Width - 1)
                {
                    Grid[c, r].Neighbours[Neighbour.TopRight] = Grid[c + 1, r - 1];
                }
                if (c < configuration.Width - 1)
                {
                    Grid[c, r].Neighbours[Neighbour.Right] = Grid[c + 1, r];
                }
                if (r < configuration.Height - 1 && c < configuration.Width - 1)
                {
                    Grid[c, r].Neighbours[Neighbour.BottomRight] = Grid[c + 1, r + 1];
                }
                if (r < configuration.Height - 1)
                {
                    Grid[c, r].Neighbours[Neighbour.Bottom] = Grid[c, r + 1];
                }
                if (r < configuration.Height - 1 && c > 0)
                {
                    Grid[c, r].Neighbours[Neighbour.BottomLeft] = Grid[c - 1, r + 1];
                }
                if (c > 0)
                {
                    Grid[c, r].Neighbours[Neighbour.Left] = Grid[c - 1, r];
                }
                if (r > 0 && c > 0)
                {
                    Grid[c, r].Neighbours[Neighbour.TopLeft] = Grid[c - 1, r - 1];
                }
            }
        }
        _stopwatch.Reset();
        IsRunning = true;
    }

    public void ClickCell(GameCell cell)
    {
        // If the cell is already exposed, do nothing
        if (cell.Exposed)
        {
            return;
        }

        //Click?.Invoke(this, EventArgs.Empty);

        if (!AllCells.Any(c => c.Exposed))
        {
            _stopwatch.Start();
            // Ensure the first clicked cell is never a bomb
            if (cell.HasBomb)
            {
                AllCells.First(c => !c.HasBomb).HasBomb = true;
                cell.HasBomb = false;
            }
            // Ensure none of the neighbours of the first clicked cell are bombs
            foreach ((Neighbour _, GameCell gameCell) in cell.Neighbours.Where(pair => pair.Value.HasBomb))
            {
                gameCell.HasBomb = false;
                AllCells
                    .Where(c => !c.Equals(cell) && !cell.Neighbours.Values.Select(nb => nb.Id).Contains(c.Id))
                    .First(c => !c.HasBomb).HasBomb = true;
            }
        }

        // Expose the clicked cell
        cell.Expose();
        // Check for win/loss conditions
        if (cell.HasBomb)
        {
            // Player clicked on a bomb, game over
            Stop(Lose);
        }
        else if (!AllCells.Any(cell => !cell.HasBomb && !cell.Exposed))
        {
            // All non-bomb cells are exposed, player wins
            Stop(Win);
        }
        else 
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        if (IsRunning)
        {
            return;
        }

        // If the game is over, expose all cells
        AllCells.Where(c => !c.Exposed).ToList().ForEach(c => c.Expose(true));
    }

    private void Stop(EventHandler? eventHandler)
    {
        _stopwatch.Stop();
        IsRunning = false;
        eventHandler?.Invoke(this, EventArgs.Empty);
    }

    public void Pause()
    {
        if (IsPaused)
        {
            return;
        }
        _stopwatch.Stop();
        IsPaused = true;
    }

    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }
        IsPaused = false;
        _stopwatch.Start();
    }
}
