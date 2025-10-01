using System.Text.Json;

namespace BoomBoom;

internal static class Program
{
    public static HighScores HighScores = new HighScores();
    public const string InfoFile = "highscores.json";

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        if (!File.Exists("highscores.json"))
        {
            Program.HighScores = Program.InitInfo();
        }
        else
        {
            string json = File.ReadAllText("highscores.json");
            try
            {
                Program.HighScores = JsonSerializer.Deserialize<HighScores>(json) ?? Program.InitInfo();
            }
            catch
            {
                int num = (int)MessageBox.Show("Could not read the information file with the time records.", "BoomBoom Info Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Program.InitInfo();
            }
        }
        // ISSUE: reference to a compiler-generated method
        ApplicationConfiguration.Initialize();
        Application.Run((Form)new MineField());
    }

    private static HighScores InitInfo()
    {
        HighScores highScores = new HighScores();
        File.WriteAllText("highscores.json", JsonSerializer.Serialize<HighScores>(highScores));
        return highScores;
    }
}


