using System;
using System.IO;
using BoomBoom;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoomTest;

[TestClass]
public class GameTests
{
    private const string StatisticsFile = "statistics.csv";

    [TestInitialize]
    public void Init()
    {
        if (File.Exists(StatisticsFile)) File.Delete(StatisticsFile);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (File.Exists(StatisticsFile)) File.Delete(StatisticsFile);
        }
        catch (IOException)
        {
            // ignore cleanup IO errors
        }
    }

    [TestMethod]
    public void FirstClick_MakesClickedCellSafe()
    {
        var cfg = new GameConfiguration { Name = "t", Width = 2, Height = 1, NumberOfBombs = 0 };
        var game = new Game(cfg);

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
        // Use a 3-cell row so we can place a bomb out of neighbour range of the first cell.
        var cfg = new GameConfiguration { Name = "t", Width = 3, Height = 1, NumberOfBombs = 1 };
        var game = new Game(cfg);

        // ensure first is safe and third is the bomb
        game.Grid[0,0].HasBomb = false;
        game.Grid[1,0].HasBomb = false;
        game.Grid[2,0].HasBomb = true;

        // first click to start the game
        var first = game.Grid[0, 0];
        game.ClickCell(first);

        bool lost = false;
        game.Lose += (_, _) => lost = true;

        // click the bomb on third cell
        var third = game.Grid[2, 0];
        game.ClickCell(third);

        Assert.IsTrue(lost, "Lose event should be raised when clicking a bomb");
        Assert.IsFalse(game.IsRunning, "Game should not be running after loss");
        Assert.IsFalse(game.Statistics.Win, "Statistics.Win should be false after loss");
        // statistics file should have been written
        Assert.IsTrue(File.Exists(StatisticsFile));
    }

    [TestMethod]
    public void SingleCell_NoBomb_TriggersWin()
    {
        var cfg = new GameConfiguration { Name = "single", Width = 1, Height = 1, NumberOfBombs = 0 };
        var game = new Game(cfg);

        bool won = false;
        game.Win += (_, _) => won = true;

        var cell = game.Grid[0, 0];
        cell.HasBomb = false;
        game.ClickCell(cell);

        Assert.IsTrue(won, "Win event should be raised for single cell with no bombs");
        Assert.IsFalse(game.IsRunning, "Game should not be running after win");
        Assert.IsTrue(game.Statistics.Win);
        Assert.IsTrue(File.Exists(StatisticsFile));
    }

    [TestMethod]
    public void FlagsPlaced_ReturnsNumberOfFlaggedCells()
    {
        var cfg = new GameConfiguration { Name = "f", Width = 2, Height = 1, NumberOfBombs = 0 };
        var game = new Game(cfg);

        game.Grid[0,0].Flagged = true;
        game.Grid[1,0].Flagged = false;

        Assert.AreEqual(1, game.FlagsPlaced);
    }

    [TestMethod]
    public void PauseAndResume_TogglesIsPaused()
    {
        var cfg = new GameConfiguration { Name = "p", Width = 2, Height = 2, NumberOfBombs = 0 };
        var game = new Game(cfg);

        Assert.IsFalse(game.IsPaused);
        game.Pause();
        Assert.IsTrue(game.IsPaused);
        game.Resume();
        Assert.IsFalse(game.IsPaused);
    }
}
