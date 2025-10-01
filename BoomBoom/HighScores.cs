using System.Text.Json;

public class HighScores
{
    public Dictionary<string, TimeSpan> Records { get; set; } = new Dictionary<string, TimeSpan>();

    public bool RecordTime(string name, TimeSpan? time)
    {
        if (!time.HasValue)
        {
            return false;
        }

        var flag = false;
        if (!this.Records.TryAdd(name, time.Value))
        {
            TimeSpan record = this.Records[name];
            TimeSpan? nullable = time;
            if (nullable.HasValue && record > nullable.GetValueOrDefault())
            {
                this.Records[name] = time.Value;
                flag = true;
            }
        }
        else
        {
            flag = true;
        }

        if (flag)
        {
            File.WriteAllText("highscores.json", JsonSerializer.Serialize<HighScores>(this, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }

        return flag;
    }
}
