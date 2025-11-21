using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using BoomBoom.Services;

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
        // Start Web API
        Task.Run(() => StartWebApi());

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

    private static void StartWebApi()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://localhost:5000");
        var app = builder.Build();

        ApiStartup.Configure(app);

        app.Run();
    }

    private static HighScores InitInfo()
    {
        HighScores highScores = new HighScores();
        File.WriteAllText("highscores.json", JsonSerializer.Serialize<HighScores>(highScores));
        return highScores;
    }
}

public record MoveRequest(int Column, int Row, string Action);


