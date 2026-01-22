using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    private IHost? _host;
    private HttpClient? _client;
    private string? _statsFile;

    [TestInitialize]
    public async Task Init()
    {
        _statsFile = Path.GetTempFileName();
        
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services => services.AddRouting())
                    .Configure(app => 
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints => ApiStartup.Configure(endpoints));
                    });
            });

        _host = await hostBuilder.StartAsync();
        _client = _host.GetTestClient();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        if (File.Exists(_statsFile)) File.Delete(_statsFile!);
    }

    [TestMethod]
    public async Task GetGame_ReturnsGameState()
    {
        var cfg = new GameConfiguration("Test", 5, 5, 0);
        var game = new Game(cfg, _statsFile!);
        GameService.Instance.RegisterGame(game);

        var response = await _client!.GetAsync("/api/game");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual(5, content.GetProperty("width").GetInt32());
        Assert.IsTrue(content.GetProperty("running").GetBoolean());
    }

    [TestMethod]
    public async Task GetGame_ReturnsStatus()
    {
        var cfg = new GameConfiguration("Test", 5, 5, 0);
        var game = new Game(cfg, _statsFile!);
        GameService.Instance.RegisterGame(game);

        var response = await _client!.GetAsync("/api/game");
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("Playing", content.GetProperty("status").GetString());

        // Trigger win
        game.ClickCell(game.Grid[0,0]);
        
        response = await _client!.GetAsync("/api/game");
        content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("Won", content.GetProperty("status").GetString());
    }

    [TestMethod]
    public async Task NewGame_TriggersEvent()
    {
        var cfg = new GameConfiguration("Test", 5, 5, 0);
        var game = new Game(cfg, _statsFile!);
        GameService.Instance.RegisterGame(game);
        
        GameConfiguration? requestedConfig = null;
        GameService.Instance.OnStartNewGame += (c) => requestedConfig = c;

        var request = new NewGameRequest("Intermediate", null, null, null);
        var response = await _client!.PostAsJsonAsync("/api/game/new", request);
        
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(requestedConfig);
        Assert.AreEqual("Intermediate", requestedConfig.Name);
        Assert.AreEqual(16, requestedConfig.Width);
    }
}
