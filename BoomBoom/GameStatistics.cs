using BoomBoom;
using System.Globalization;

public class GameStatistics 
{
    private readonly GameConfiguration _gameConfiguration;

    public GameStatistics(GameConfiguration gameConfiguration)
    {
        _gameConfiguration = gameConfiguration;
    }

    public bool Win { get; set; }
    public TimeSpan? PlayTime { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime StopDateTime { get; set; }
    public int CellsCleared { get; set; }
    public int Clicks { get; set; }


    public string Name => _gameConfiguration.Name;
    public int Height => _gameConfiguration.Height;
    public int Width => _gameConfiguration.Width;
    public int NumberOfBombs => _gameConfiguration.NumberOfBombs;

    public override string ToString()
    {
        return $"{Name},{Height},{Width},{NumberOfBombs},{Win},{PlayTime},{StartDateTime},{StopDateTime},{CellsCleared},{Clicks}";
    }

    public static GameStatistics FromString(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Input string is null or empty.", nameof(s));

        // Split into comma-separated tokens, then split each token at first ": "
        var parts = s.Split(new[] { "," }, StringSplitOptions.None);

        // Build GameConfiguration
        var config = new GameConfiguration
        {
            Name = parts[0],
            Height = int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var h) ? h : 0,
            Width = int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var w) ? w : 0,
            NumberOfBombs = int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var b) ? b : 0
        };

        var stats = new GameStatistics(config);

        if (parts.Length > 4 && bool.TryParse(parts[4], out var win))
            stats.Win = win;

        if (parts.Length > 5 && TimeSpan.TryParse(parts[5], out var playTime))
            stats.PlayTime = playTime;

        if (parts.Length > 6 && DateTime.TryParse(parts[6], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out var startDt))
            stats.StartDateTime = startDt;

        if (parts.Length > 7 && DateTime.TryParse(parts[7], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out var stopDt))
            stats.StopDateTime = stopDt;

        if (parts.Length > 8 && int.TryParse(parts[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out var cells))
            stats.CellsCleared = cells;

        if (parts.Length > 9 && int.TryParse(parts[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out var clicks))
            stats.Clicks = clicks;

        return stats;
    }

    public static GameStatistics FromString2(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Input string is null or empty.", nameof(s));

        // Split into comma-separated tokens, then split each token at first ": "
        var parts = s.Split(new[] { ", " }, StringSplitOptions.None);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in parts)
        {
            var idx = part.IndexOf(": ", StringComparison.Ordinal);
            if (idx <= 0) // no key/value separator
                continue;
            var key = part.Substring(0, idx).Trim();
            var value = part.Substring(idx + 2).Trim();
            dict[key] = value;
        }

        // Build GameConfiguration
        var config = new GameConfiguration
        {
            Name = dict.TryGetValue("Name", out var name) ? name : string.Empty,
            Height = dict.TryGetValue("Height", out var hstr) && int.TryParse(hstr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var h) ? h : 0,
            Width = dict.TryGetValue("Width", out var wstr) && int.TryParse(wstr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var w) ? w : 0,
            NumberOfBombs = dict.TryGetValue("NumberOfBombs", out var bstr) && int.TryParse(bstr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var b) ? b : 0
        };

        var stats = new GameStatistics(config);

        if (dict.TryGetValue("Win", out var winStr) && bool.TryParse(winStr, out var win))
            stats.Win = win;

        if (dict.TryGetValue("PlayTime", out var playTimeStr))
        {
            // empty means null (ToString of nullable TimeSpan yields empty when null)
            if (string.IsNullOrEmpty(playTimeStr))
                stats.PlayTime = null;
            else if (TimeSpan.TryParse(playTimeStr, out var ts))
                stats.PlayTime = ts;
        }

        if (dict.TryGetValue("StartDateTime", out var startStr) && DateTime.TryParse(startStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out var startDt))
            stats.StartDateTime = startDt;

        if (dict.TryGetValue("StopDateTime", out var stopStr) && DateTime.TryParse(stopStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out var stopDt))
            stats.StopDateTime = stopDt;

        if (dict.TryGetValue("CellsCleared", out var cellsStr) && int.TryParse(cellsStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var cells))
            stats.CellsCleared = cells;

        if (dict.TryGetValue("Clicks", out var clicksStr) && int.TryParse(clicksStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var clicks))
            stats.Clicks = clicks;

        return stats;
    }

}
