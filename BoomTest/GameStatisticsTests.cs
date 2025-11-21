using System.Globalization;
using BoomBoom;

namespace BoomTest;

[TestClass]
public class GameStatisticsTests
{
        [TestMethod]
        public void FromString_ThrowsOnNullOrWhitespace()
        {
            try
            {
                GameStatistics.FromString(null!);
                Assert.Fail("Expected ArgumentException for null input");
            }
            catch (ArgumentException) { }

            try
            {
                GameStatistics.FromString(string.Empty);
                Assert.Fail("Expected ArgumentException for empty input");
            }
            catch (ArgumentException) { }

            try
            {
                GameStatistics.FromString("  ");
                Assert.Fail("Expected ArgumentException for whitespace input");
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void FromString_ParsesFullCsv()
        {
            var name = "TestGame";
            var height = 9;
            var width = 9;
            var bombs = 10;
            var win = true;
            var play = TimeSpan.FromSeconds(5);
            var start = DateTime.SpecifyKind(new DateTime(2020, 1, 1, 12, 0, 0), DateTimeKind.Utc);
            var stop = start.AddSeconds(5);
            var cells = 20;
            var clicks = 7;

            var s = string.Join(",", new[] {
                name,
                height.ToString(CultureInfo.InvariantCulture),
                width.ToString(CultureInfo.InvariantCulture),
                bombs.ToString(CultureInfo.InvariantCulture),
                win.ToString(),
                play.ToString(),
                start.ToString("o", CultureInfo.InvariantCulture),
                stop.ToString("o", CultureInfo.InvariantCulture),
                cells.ToString(CultureInfo.InvariantCulture),
                clicks.ToString(CultureInfo.InvariantCulture)
            });

            var stats = GameStatistics.FromString(s);

            Assert.AreEqual(name, stats.Name);
            Assert.AreEqual(height, stats.Height);
            Assert.AreEqual(width, stats.Width);
            Assert.AreEqual(bombs, stats.NumberOfBombs);
            Assert.AreEqual(win, stats.Win);
            Assert.AreEqual(play, stats.PlayTime);
            Assert.AreEqual(start, stats.StartDateTime);
            Assert.AreEqual(stop, stats.StopDateTime);
            Assert.AreEqual(cells, stats.CellsCleared);
            Assert.AreEqual(clicks, stats.Clicks);
        }

        [TestMethod]
        public void FromString_PartialCsv_UsesDefaults()
        {
            var s = "OnlyName,1,2,3";
            var stats = GameStatistics.FromString(s);

            Assert.AreEqual("OnlyName", stats.Name);
            Assert.AreEqual(1, stats.Height);
            Assert.AreEqual(2, stats.Width);
            Assert.AreEqual(3, stats.NumberOfBombs);
            Assert.IsFalse(stats.Win);
            Assert.IsNull(stats.PlayTime);
            Assert.AreEqual(default, stats.StartDateTime);
            Assert.AreEqual(default, stats.StopDateTime);
            Assert.AreEqual(0, stats.CellsCleared);
            Assert.AreEqual(0, stats.Clicks);
        }

        [TestMethod]
        public void FromString2_ThrowsOnNullOrWhitespace()
        {
            try
            {
                GameStatistics.FromString2(null!);
                Assert.Fail("Expected ArgumentException for null input");
            }
            catch (ArgumentException) { }

            try
            {
                GameStatistics.FromString2(string.Empty);
                Assert.Fail("Expected ArgumentException for empty input");
            }
            catch (ArgumentException) { }

            try
            {
                GameStatistics.FromString2("\t");
                Assert.Fail("Expected ArgumentException for whitespace input");
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void FromString2_ParsesKeyValueStyle_WithEmptyPlayTime()
        {
            var start = DateTime.SpecifyKind(new DateTime(2021, 6, 2, 8, 30, 0), DateTimeKind.Utc);
            var stop = start.AddSeconds(10);

            var s = string.Join(", ", new[] {
                "Name: KVTest",
                "Height: 2",
                "Width: 3",
                "NumberOfBombs: 1",
                "Win: True",
                "PlayTime: ",
                $"StartDateTime: {start.ToString("o", CultureInfo.InvariantCulture)}",
                $"StopDateTime: {stop.ToString("o", CultureInfo.InvariantCulture)}",
                "CellsCleared: 5",
                "Clicks: 9"
            });

            var stats = GameStatistics.FromString2(s);

            Assert.AreEqual("KVTest", stats.Name);
            Assert.AreEqual(2, stats.Height);
            Assert.AreEqual(3, stats.Width);
            Assert.AreEqual(1, stats.NumberOfBombs);
            Assert.IsTrue(stats.Win);
            Assert.IsNull(stats.PlayTime);
            Assert.AreEqual(start, stats.StartDateTime);
            Assert.AreEqual(stop, stats.StopDateTime);
            Assert.AreEqual(5, stats.CellsCleared);
            Assert.AreEqual(9, stats.Clicks);
        }
    }

