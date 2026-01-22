using System;
using System.IO;
using BoomBoom;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoomTest;

[TestClass]
    public class GameTests
    {
        private string? _statisticsFile;

        [TestInitialize]
        public void Init()
        {
            _statisticsFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".csv");
            if (File.Exists(_statisticsFile)) File.Delete(_statisticsFile);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (File.Exists(_statisticsFile)) File.Delete(_statisticsFile);
            }
            catch (IOException)
            {
                // ignore cleanup IO errors
            }
        }
    [TestMethod]
    public void FirstClick_MakesClickedCellSafe()
    {
        var cfg = new GameConfiguration("t", 1, 2, 0);
        var game = new Game(cfg, _statisticsFile!);

        // force a bomb on cell 0
        game.Grid[0, 0].HasBomb = true;
        game.Grid[1, 0].HasBomb = false;

        // first click on cell[0,0] should make it safe
        game.ClickCell(game.Grid[0,0]);

        Assert.IsFalse(game.Grid[0,0].HasBomb, "First clicked cell should be made safe");
        Assert.IsTrue(game.Grid[0,0].Exposed, "First clicked cell should be exposed");
    }

    [TestMethod]
    public void ClickBomb_TriggersLose_WhenBombClickedAfterFirstClick()
    {
        // Use a 5x5 grid with a single bomb to ensure the first click doesn't disarm the bomb
        // and the game remains running for a subsequent bomb click.
        var cfg = new GameConfiguration("t", 5, 5, 0);
        var game = new Game(cfg, _statisticsFile!);

        // Place the single bomb at [4,4]
        game.Grid[4, 4].HasBomb = true;

        // Ensure other cells are safe
        game.Grid[0, 0].HasBomb = false;

        // first click to start the game on a safe cell far from the bomb
        var first = game.Grid[0, 0];
        game.ClickCell(first);

        // Assert that the game is still running after the first click (no win/loss yet)
        Assert.IsTrue(game.IsRunning, "Game should still be running after the first safe click.");

        bool lost = false;
        game.Lose += (_, _) => lost = true;

        // click a bomb cell that should trigger a loss
        var bombCell = game.Grid[4, 4];
        game.ClickCell(bombCell);

        Assert.IsTrue(lost, "Lose event should be raised when clicking a bomb");
        Assert.IsFalse(game.IsRunning, "Game should not be running after loss");
        Assert.IsFalse(game.Statistics.Win, "Statistics.Win should be false after loss");
        Assert.IsTrue(File.Exists(_statisticsFile));
    }

    [TestMethod]
    public void SingleCell_NoBomb_TriggersWin()
    {
        var cfg = new GameConfiguration("single", 1, 1, 0);
        var game = new Game(cfg, _statisticsFile!);

        bool won = false;
        game.Win += (_, _) => won = true;

        var cell = game.Grid[0, 0];
        cell.HasBomb = false;
        game.ClickCell(cell);

        Assert.IsTrue(won, "Win event should be raised for single cell with no bombs");
        Assert.IsFalse(game.IsRunning, "Game should not be running after win");
        Assert.IsTrue(game.Statistics.Win);
        Assert.IsTrue(File.Exists(_statisticsFile));
    }

    [TestMethod]
    public void FlagsPlaced_ReturnsNumberOfFlaggedCells()
    {
        var cfg = new GameConfiguration ("f", 1, 2, 0);
        var game = new Game(cfg);

        game.Grid[0,0].Flagged = true;
        game.Grid[1,0].Flagged = false;

        Assert.AreEqual(1, game.FlagsPlaced);
    }

    [TestMethod]
    public void PauseAndResume_TogglesIsPaused()
    {
        var cfg = new GameConfiguration ("p", 2, 2, 0);
        var game = new Game(cfg);

        Assert.IsFalse(game.IsPaused);
        game.Pause();
        Assert.IsTrue(game.IsPaused);
        game.Resume();
        Assert.IsFalse(game.IsPaused);
    }
}
