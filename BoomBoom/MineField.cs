using BoomBoom.Properties;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Media;
using System.Timers;

namespace BoomBoom
{
    public partial class MineField : Form
    {
        private Game _game;
        private readonly System.Timers.Timer _timer;
        private GameConfiguration _currentConfiguration;
        private readonly BombTile[] _heap;
        private const int TileSize = 30;

        public MineField()
        {
            var length = 500;
            _heap = new BombTile[length];
            for (int index = 0; index < length; ++index)
            {
                _heap[index] = new BombTile();
                _heap[index].Size = new Size(TileSize, TileSize);
                _heap[index].Clicked += new BombTileClick(TileClicked);
                _heap[index].Flagged += new BombTileClick(TileFlagged);
            }

            InitializeComponent();

            _timer = new System.Timers.Timer(500.0);
            _timer.AutoReset = true;
            _timer.Elapsed += new ElapsedEventHandler(TimerOnElapsed);
            _timer.Start();
            _currentConfiguration = new GameConfiguration();
            _game = new Game(_currentConfiguration);
        }

        private void BoomForm_Load(object sender, EventArgs e) => StartBeginnerGame();

        private void mnuGameBeginner_Click(object sender, EventArgs e) => StartBeginnerGame();

        private void mnuGameIntermediate_Click(object sender, EventArgs e)
        {
            StartIntermediateGame();
        }

        private void mnuGameExpert_Click(object sender, EventArgs e) => StartExpertGame();

        private void mnuRestart_Click(object sender, EventArgs e)
        {
            StartGame(_currentConfiguration);
        }

        private void mnuHighScores_Click(object sender, EventArgs e)
        {
            HighScores.Left = (Width - HighScores.Width) / 2;
            var message = string.Join("\r\n", Program.HighScores.Records.Select(pair => $"{pair.Key}\r\n{pair.Value}"));
            Debug.WriteLine(message);
            HighScoreLabel.Text = message;
            HighScores.Show();
        }

        private void HighScores_Click(object sender, EventArgs e) => HighScores.Hide();

        private void BoomForm_Activated(object sender, EventArgs e)
        {
            if (_game.IsRunning)
            {
                _game.Resume();
            }
        }

        private void BoomForm_Deactivate(object sender, EventArgs e)
        {
            if (_game.IsRunning)
            {
                _game.Pause();
            }
        }

        private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            if (_game.IsRunning)
            {
                mnuTimer.Text = $"{_game.GameTime.Minutes:00}:{_game.GameTime.Seconds:00}";
            }
        }

        private void StartBeginnerGame()
        {
            _currentConfiguration = new GameConfiguration()
            {
                Name = "Beginner",
                Height = 9,
                Width = 9,
                NumberOfBombs = 10
            };
            StartGame(_currentConfiguration);
        }

        private void StartIntermediateGame()
        {
            _currentConfiguration = new GameConfiguration()
            {
                Name = "Intermediate",
                Height = 16 /*0x10*/,
                Width = 16 /*0x10*/,
                NumberOfBombs = 40
            };
            StartGame(_currentConfiguration);
        }

        private void StartExpertGame()
        {
            _currentConfiguration = new GameConfiguration()
            {
                Name = "Expert",
                Height = 16 /*0x10*/,
                Width = 30,
                NumberOfBombs = 99
            };
            StartGame(_currentConfiguration);
        }

        private void StartGame(GameConfiguration configuration)
        {
            _game = new Game(configuration);
            InitializeGame(_game);
            _game.Lose += (_, _) => Kaboom();
            _game.Win += (_, _) => Win();
            _game.Click += (_, _) => Boop();
        }

        private void Win()
        {
            if (SoundOn)
            {
                AsyncSound(Resources.Tadaa);
            }
            var gametime = $"{_game.GameTime.Minutes:00}:{_game.GameTime.Seconds:00}.{_game.GameTime.Milliseconds:000}";
            var message =
                Program.HighScores.RecordTime(_currentConfiguration.Name, new TimeSpan?(_game.GameTime))
                    ? $"WOOHOO!! A new record! Time: {gametime}"
                    : $"CONGRATS! You won! Time: {gametime}";
            
            AsyncMessage(message);
        }

        private bool SoundOn => mnuSound.Checked;

        private void Kaboom()
        {
            if (SoundOn)
            {
                AsyncSound(Resources.ExplosionSound);
            }
        }

        private void Boop()
        {
            if (SoundOn)
            {
                AsyncSound(Resources.Boop);
            }
        }

        private static void AsyncMessage(string message)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (_, _) => MessageBox.Show(message);
            backgroundWorker.RunWorkerAsync();
        }

        private static void AsyncSound(UnmanagedMemoryStream stream)
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (_, _) => new SoundPlayer(stream).Play();
            backgroundWorker.RunWorkerAsync();
        }

        private void InitializeGame(Game game)
        {
            SuspendLayout();
            Controls.Cast<Control>().Where(c => c is BombTile).ToList().ForEach(new Action<Control>(Controls.Remove));
            Width = game.Configuration.Width * 30 + 18;
            Height = game.Configuration.Height * 30 + 30 + 45;
            var controlList = new List<Control>();
            for (int c = 0; c < game.Configuration.Width; ++c)
            {
                for (int r = 0; r < game.Configuration.Height; ++r)
                {
                    var bombTile = _heap[c * game.Configuration.Height + r];
                    bombTile.Initialize(game.Grid[c, r]);
                    bombTile.Left = c * 30;
                    bombTile.Top = 30 + r * 30;
                    controlList.Add(bombTile);
                }
            }
            Controls.AddRange(controlList.ToArray());
            mnuBombCount.Text = $"{game.Configuration.NumberOfBombs - game.FlagsPlaced}";
            ResumeLayout(false);
            PerformLayout();
        }

        private void TileClicked(BombTile sender, GameCell cell)
        {
            if (!cell.Exposed)
            {
                _game.ClickCell(cell);
            }
        }

        private void TileFlagged(BombTile sender, GameCell cell)
        {
            mnuBombCount.Text = $"{_game.Configuration.NumberOfBombs - _game.FlagsPlaced}";
        }

    }
}
