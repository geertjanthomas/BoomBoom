namespace BoomBoom;

public class GameConfiguration
{
    public GameConfiguration()
    {
        
    }

    public GameConfiguration(string name, int height, int width, int numberOfBombs)
    {
        Name = name;
        Height = height;
        Width = width;
        NumberOfBombs = numberOfBombs;
    }

    public string Name { get; init; } = string.Empty;

    public int Height { get; }

    public int Width { get; }

    public int NumberOfBombs { get; }
}
