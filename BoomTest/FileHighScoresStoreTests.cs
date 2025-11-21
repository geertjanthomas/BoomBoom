using BoomBoom;

namespace BoomTest;

[TestClass]
public class FileHighScoresStoreTests
{
        private static string TempPathForTest() => Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        [TestMethod]
        [TestCategory("Integration")]
        public void Save_WritesFile_WithSerializedContent()
        {
            var path = TempPathForTest();
            try
            {
                var store = new FileHighScoresStore(path);
                var hs = new HighScores
                {
                    Records = new Dictionary<string, TimeSpan>
                    {
                        ["level1"] = TimeSpan.FromSeconds(12)
                    }
                };

                // call store directly
                store.Save(hs);

                Assert.IsTrue(File.Exists(path), "File should exist after save");
                var content = File.ReadAllText(path);
                Assert.Contains("Records", content, "Serialized content should contain Records property");
                Assert.Contains("level1", content, "Serialized content should contain the record key");
                Assert.Contains("00:00:12", content, "Serialized content should contain the timespan value");
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Save_CanBeCalledConcurrently_WithoutThrowing()
        {
            var path = TempPathForTest();
            try
            {
                var store = new FileHighScoresStore(path);

                var tasks = Enumerable.Range(0, 20).Select(i => Task.Run(() =>
                {
                    var hs = new HighScores
                    {
                        Records = new Dictionary<string, TimeSpan>
                        {
                            [$"r{i}"] = TimeSpan.FromMilliseconds(i)
                        }
                    };
                    store.Save(hs);
                }, TestContext.CancellationToken)).ToArray();

                Task.WaitAll(tasks, TestContext.CancellationToken);

                Assert.IsTrue(File.Exists(path), "File should exist after concurrent saves");
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        public TestContext TestContext { get; set; }
}
