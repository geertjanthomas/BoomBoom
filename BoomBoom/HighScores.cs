namespace BoomBoom;

using System;
using System.Collections.Generic;

public class HighScores(IHighScoresStore? store = null)
{
    public Dictionary<string, TimeSpan> Records { get; set; } = [];

    public bool RecordTime(string name, TimeSpan? time)
    {
        if (!time.HasValue)
        {
            return false;
        }

        var flag = false;
        if (!Records.TryAdd(name, time.Value))
        {
            TimeSpan record = Records[name];
            TimeSpan? nullable = time;
            if (nullable.HasValue && record > nullable.GetValueOrDefault())
            {
                Records[name] = time.Value;
                flag = true;
            }
        }
        else
        {
            flag = true;
        }

        if (flag)
        {
            if (store is not null)
            {
                store.Save(this);
            }
            else
            {
                // default behavior for existing consumers
                var fileStore = new FileHighScoresStore();
                fileStore.Save(this);
            }
        }

        return flag;
    }
}
