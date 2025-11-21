
namespace BoomBoom;

public class GameCell
{
    public string Id => $"{this.Column}:{this.Row}";

    public int Column { get; init; }

    public int Row { get; init; }

    public bool HasBomb { get; set; }

    public bool Flagged { get; set; }

    public bool Exposed { get; set; }

    public IBombTile? Tile { get; set; }

    public int NumberOfAdjacentBombs
    {
        get => this.Neighbours.Values.Count(cell => cell.HasBomb);
    }

    public Dictionary<Neighbour, GameCell> Neighbours { get; } = new Dictionary<Neighbour, GameCell>();

    public void Expose(bool endOfGame = false)
    {
        this.Exposed = true;
        this.Tile?.Expose(endOfGame);

        if (endOfGame || this.NumberOfAdjacentBombs != 0)
        {
            return;
        }

        foreach ((Neighbour _, GameCell gameCell) in this.Neighbours)
        {
            if (!gameCell.Exposed)
            {
                gameCell.Expose();
            }
        }
    }

    public override string ToString()
    {
        return $"{(this.HasBomb ? "B" : "-")} - {this.Column},{this.Row} - {string.Join(" - ", this.Neighbours.Values.Select(c =>
        {
            string str = c.HasBomb ? "X" : "o";
            return $"{c.Column},{c.Row},{str}";
        }))}";
    }

    public override bool Equals(object? obj) => obj is GameCell gameCell && gameCell.Id == this.Id;

    public override int GetHashCode() => this.Id.GetHashCode();
}
