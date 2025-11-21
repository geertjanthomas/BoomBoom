using BoomBoom;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoomTest;

[TestClass]
public class GameCellTests
{
    [TestMethod]
    public void Id_FormatsColumnAndRow()
    {
        var cell = new GameCell { Column = 2, Row = 3 };
        Assert.AreEqual("2:3", cell.Id);
    }

    [TestMethod]
    public void NumberOfAdjacentBombs_CountsBombNeighbours()
    {
        var c = new GameCell { Column = 1, Row = 1 };
        var left = new GameCell { Column = 0, Row = 1, HasBomb = true };
        var right = new GameCell { Column = 2, Row = 1, HasBomb = false };
        var top = new GameCell { Column = 1, Row = 0, HasBomb = true };

        c.Neighbours[Neighbour.Left] = left;
        c.Neighbours[Neighbour.Right] = right;
        c.Neighbours[Neighbour.Top] = top;

        Assert.AreEqual(2, c.NumberOfAdjacentBombs);
    }

    [TestMethod]
    public void Expose_SetsExposedFlag()
    {
        var cell = new GameCell { Column = 0, Row = 0 };
        Assert.IsFalse(cell.Exposed);
        cell.Expose();
        Assert.IsTrue(cell.Exposed);
    }

    [TestMethod]
    public void Expose_CallsTileExpose_OnAttachedTile()
    {
        var cell = new GameCell { Column = 0, Row = 0 };
        var tile = new TestBombTile();
        cell.Tile = tile;

        cell.Expose();

        Assert.IsTrue(tile.ExposedCalled);
        Assert.IsFalse(tile.EndOfGameFlag);
    }

    [TestMethod]
    public void Expose_EndOfGame_PassesFlagToTile_AndDoesNotPropagate()
    {
        var a = new GameCell { Column = 0, Row = 0 };
        var b = new GameCell { Column = 1, Row = 0 };
        a.Neighbours[Neighbour.Right] = b;
        b.Neighbours[Neighbour.Left] = a;

        var tileA = new TestBombTile();
        var tileB = new TestBombTile();
        a.Tile = tileA;
        b.Tile = tileB;

        a.Expose(endOfGame: true);

        Assert.IsTrue(tileA.ExposedCalled);
        Assert.IsTrue(tileA.EndOfGameFlag);
        Assert.IsFalse(b.Exposed, "End of game expose should not automatically expose neighbours");
        Assert.IsFalse(tileB.ExposedCalled, "Tile on neighbour should not have been exposed when endOfGame is true on source");
    }

    [TestMethod]
    public void Expose_PropagatesToNeighbours_WhenNoAdjacentBombs()
    {
        var a = new GameCell { Column = 0, Row = 0 };
        var b = new GameCell { Column = 1, Row = 0 };
        var c = new GameCell { Column = 2, Row = 0 };

        a.Neighbours[Neighbour.Right] = b;
        b.Neighbours[Neighbour.Left] = a;
        b.Neighbours[Neighbour.Right] = c;
        c.Neighbours[Neighbour.Left] = b;

        a.Expose();

        Assert.IsTrue(a.Exposed);
        Assert.IsTrue(b.Exposed);
        Assert.IsTrue(c.Exposed);
    }

    [TestMethod]
    public void Expose_DoesNotPropagate_WhenCellHasAdjacentBombs()
    {
        var center = new GameCell { Column = 1, Row = 1 };
        var neighbourBomb = new GameCell { Column = 0, Row = 1, HasBomb = true };
        var neighbourSafe = new GameCell { Column = 2, Row = 1, HasBomb = false };

        center.Neighbours[Neighbour.Left] = neighbourBomb;
        center.Neighbours[Neighbour.Right] = neighbourSafe;

        center.Expose();

        Assert.IsTrue(center.Exposed);
        Assert.IsFalse(neighbourSafe.Exposed, "Expose should not propagate when the exposed cell has adjacent bombs");
    }

    [TestMethod]
    public void ToString_IncludesBombIndicatorAndNeighbours()
    {
        var cell = new GameCell { Column = 5, Row = 6, HasBomb = true };
        var nb = new GameCell { Column = 4, Row = 6, HasBomb = false };
        cell.Neighbours[Neighbour.Left] = nb;

        var s = cell.ToString();

        Assert.StartsWith("B", s, "ToString should start with 'B' when cell has a bomb");
        Assert.Contains("5,6", s);
        Assert.Contains("4,6", s);
    }

    [TestMethod]
    public void Equals_UsesId()
    {
        var a = new GameCell { Column = 2, Row = 2 };
        var b = new GameCell { Column = 2, Row = 2 };
        var c = new GameCell { Column = 3, Row = 2 };

        Assert.IsTrue(a.Equals(b));
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        Assert.IsFalse(a.Equals(c));
    }
}
