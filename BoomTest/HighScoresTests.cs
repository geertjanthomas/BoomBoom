using BoomBoom;

namespace BoomTest;

[TestClass]
public class HighScoresTests
{
    private class FakeStore : IHighScoresStore
    {
        public HighScores? Saved { get; private set; }
        public void Save(HighScores highscores) => Saved = highscores;
    }

    [TestMethod]
    public void RecordTime_ReturnsFalse_WhenTimeIsNull()
    {
        var store = new FakeStore();
        var hs = new HighScores(store)
        {
            Records = []
        };

        var result = hs.RecordTime("level1", null);

        Assert.IsFalse(result);
        Assert.IsNull(store.Saved, "No save should be performed when time is null");
    }

    [TestMethod]
    public void RecordTime_AddsNewRecord_AndWritesFile()
    {
        var store = new FakeStore();
        var hs = new HighScores(store)
        {
            Records = []
        };

        var result = hs.RecordTime("levelA", TimeSpan.FromSeconds(12));

        Assert.IsTrue(result, "New record should return true");
        Assert.IsTrue(hs.Records.ContainsKey("levelA"));
        Assert.AreEqual(TimeSpan.FromSeconds(12), hs.Records["levelA"]);
        Assert.IsNotNull(store.Saved, "Store.Save should have been called");
    }

    [TestMethod]
    public void RecordTime_UpdatesOnlyWhenBetter()
    {
        var store = new FakeStore();
        var hs = new HighScores(store)
        {
            Records = new Dictionary<string, TimeSpan>
            {
                ["boss"] = TimeSpan.FromSeconds(20)
            }
        };

        // Better time -> should update
        var better = hs.RecordTime("boss", TimeSpan.FromSeconds(15));
        Assert.IsTrue(better);
        Assert.AreEqual(TimeSpan.FromSeconds(15), hs.Records["boss"]);

        // Worse time -> should not update
        var worse = hs.RecordTime("boss", TimeSpan.FromSeconds(30));
        Assert.IsFalse(worse);
        Assert.AreEqual(TimeSpan.FromSeconds(15), hs.Records["boss"]);
    }
}
