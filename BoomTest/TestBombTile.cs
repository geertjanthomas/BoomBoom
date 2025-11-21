using System;
using BoomBoom;

namespace BoomTest;

public class TestBombTile : IBombTile
{
    public bool ExposedCalled { get; private set; }
    public bool EndOfGameFlag { get; private set; }

    public void Expose(bool endOfGame = false)
    {
        ExposedCalled = true;
        EndOfGameFlag = endOfGame;
    }
}
