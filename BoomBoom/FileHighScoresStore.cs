using System.Text.Json;

namespace BoomBoom;

public class FileHighScoresStore(string filePath = "highscores.json") : IHighScoresStore
{
    private readonly string _filePath = filePath;
    private static readonly Lock _fileLock = new();

    public void Save(HighScores highscores)
    {
        lock (_fileLock)
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize<HighScores>(highscores, JsonOptions));
        }
    }

    private static JsonSerializerOptions JsonOptions => new() { WriteIndented = true };
}
