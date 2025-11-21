using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using BoomBoom;
using BoomBoom.Services;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO;

namespace BoomTest;

[TestClass]
[DoNotParallelize]
public class ApiTests
{
    private TestServer? _server;
    private HttpClient? _client;
    private string? _statsFile;

    [TestInitialize]
    public void Init()
    {
        _statsFile = Path.GetTempFileName();
        
        var hostBuilder = new WebHostBuilder()
            .ConfigureServices(services => services.AddRouting())
            .Configure(app => 
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => ApiStartup.Configure(endpoints));
            });

        _server = new TestServer(hostBuilder);
        _client = _server.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _server?.Dispose();
        if (File.Exists(_statsFile)) File.Delete(_statsFile!);
    }

    [TestMethod]
    public async Task GetGame_ReturnsGameState()
    {
        var cfg = new GameConfiguration { Width = 5, Height = 5, NumberOfBombs = 0 };
        var game = new Game(cfg, _statsFile!);
        GameService.Instance.RegisterGame(game, null);

        var response = await _client!.GetAsync("/api/game");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual(5, content.GetProperty("width").GetInt32());
        Assert.IsTrue(content.GetProperty("running").GetBoolean());
    }

    [TestMethod]
    public async Task MakeMove_Click_UpdatesGame()
    {
        var cfg = new GameConfiguration { Width = 5, Height = 5, NumberOfBombs = 0 };
        var game = new Game(cfg, _statsFile!);
        GameService.Instance.RegisterGame(game, null);

        // Grid[0,0] is not exposed
        Assert.IsFalse(game.Grid[0,0].Exposed);

        var move = new MoveRequest(0, 0, "click");
        var response = await _client!.PostAsJsonAsync("/api/game/move", move);
        
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        // Check game state directly (since GameService holds ref to same object)
        Assert.IsTrue(game.Grid[0,0].Exposed);
    }
}
