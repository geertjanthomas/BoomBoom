using System.Collections;
using System.Diagnostics;

namespace BoomBoom;

    public class Game
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _statisticsFilePath;

        public bool IsRunning { get; private set; }

        public bool IsPaused { get; private set; }

        public GameConfiguration Configuration { get; }

        public GameCell[,] Grid { get; }

        public TimeSpan GameTime => _stopwatch.Elapsed;

        public GameStatistics Statistics { get; }

        public event EventHandler? Lose;

        public event EventHandler? Win;

        public event EventHandler? Click;

        private IEnumerable<GameCell> AllCells => ((IEnumerable)Grid).Cast<GameCell>();

        public int FlagsPlaced => AllCells.Count(c => c.Flagged);

        public Game(GameConfiguration configuration, string statisticsFilePath = "statistics.csv")
        {
            _stopwatch = new Stopwatch();
            Configuration = configuration;
            _statisticsFilePath = statisticsFilePath;
            Statistics = new GameStatistics(configuration)
        {
            Win = false,
            PlayTime = null,
            StartDateTime = DateTime.Now,
            StopDateTime = DateTime.MinValue,
            CellsCleared = 0,
            Clicks = 0
        };

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
                var target = AllCells.FirstOrDefault(c => !c.HasBomb);
                if (target is not null)
                {
                    target.HasBomb = true;
                }
                // ensure the clicked cell is safe
                cell.HasBomb = false;
            }
            // Ensure none of the neighbours of the first clicked cell are bombs
                foreach ((Neighbour _, GameCell gameCell) in cell.Neighbours.Where(pair => pair.Value.HasBomb))
                {
                    gameCell.HasBomb = false;
                    var replacement = AllCells
                        .Where(c => !c.Equals(cell) && !cell.Neighbours.Values.Select(nb => nb.Id).Contains(c.Id))
                        .FirstOrDefault(c => !c.HasBomb);
                    if (replacement is not null)
                    {
                        replacement.HasBomb = true;
                    }
                }
        }

        // Expose the clicked cell
        Statistics.Clicks++;
        cell.Expose();
        // Check for win/loss conditions
        if (cell.HasBomb)
        {
            // Player clicked on a bomb, game over
            Statistics.Win = false;
            Stop(Lose);
        }
        else if (AllCells.Count(c => c.Exposed && !c.HasBomb) == (Configuration.Width * Configuration.Height - Configuration.NumberOfBombs))
        {
            // All non-bomb cells are exposed, player wins
            Statistics.Win = true;
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
        Statistics.PlayTime = _stopwatch.Elapsed;
        Statistics.StopDateTime = DateTime.Now;
        Statistics.CellsCleared = AllCells.Count(c => c.Exposed);
        IsRunning = false;
        eventHandler?.Invoke(this, EventArgs.Empty);

        try
        {
            if (!File.Exists(_statisticsFilePath))
            {
                // Write the header line of the statistics file
                File.WriteAllLines(_statisticsFilePath, new[] { "Name,Height,Width,NumberOfBombs,Win,PlayTime,StartDateTime,StopDateTime,CellsCleared,Clicks" });
            }
            File.AppendAllLines(_statisticsFilePath, new[] { Statistics.ToString() });
        }
        catch (IOException)
        {
            // Ignore IO errors when writing statistics during tests or concurrent runs
        }
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
