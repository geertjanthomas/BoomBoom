using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using BoomBoom.Services;

namespace BoomBoom;

public static class ApiStartup
{
    public static void Configure(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/game", () => {
            var game = GameService.Instance.GetGame();
            if (game == null) return Results.NotFound("No game running");
            
            var grid = new List<object>();
            for(int c=0; c<game.Configuration.Width; c++) {
                for(int r=0; r<game.Configuration.Height; r++) {
                    var cell = game.Grid[c,r];
                    grid.Add(new {
                        Column = c,
                        Row = r,
                        Exposed = cell.Exposed,
                        Flagged = cell.Flagged,
                        Adjacent = cell.Exposed ? (int?)cell.NumberOfAdjacentBombs : null
                    });
                }
            }
            
            return Results.Ok(new {
                Status = game.IsRunning ? "Playing" : (game.Statistics.Win ? "Won" : "Lost"),
                Running = game.IsRunning,
                Paused = game.IsPaused,
                Won = game.Statistics.Win,
                Width = game.Configuration.Width,
                Height = game.Configuration.Height,
                Bombs = game.Configuration.NumberOfBombs,
                Grid = grid
            });
        });

        app.MapPost("/api/game/new", (NewGameRequest? request) => {
            var config = new GameConfiguration();
            if (request != null && !string.IsNullOrEmpty(request.Difficulty))
            {
                switch (request.Difficulty.ToLower())
                {
                    case "beginner":
                        config = new GameConfiguration("Beginner", 9, 9, 10);
                        break;
                    case "intermediate":
                        config = new GameConfiguration("Intermediate", 16, 16, 40);
                        break;
                    case "expert":
                        config = new GameConfiguration("Expert", 30, 16, 99);
                        break;
                    default:
                        return Results.BadRequest("Invalid difficulty. Use Beginner, Intermediate, or Expert.");
                }
            }
            else if (request != null && request.Width.HasValue && request.Height.HasValue && request.Bombs.HasValue)
            {
                config = new GameConfiguration("Custom", request.Height.Value, request.Width.Value, request.Bombs.Value);
            }
            
            // Invoke request on UI thread via event
            GameService.Instance.RequestNewGame(config);
            return Results.Ok();
        });

        app.MapPost("/api/game/move", async (MoveRequest request) => {
            var game = GameService.Instance.GetGame();
            if (game == null) return Results.NotFound("No game running");
            
            if (request.Column < 0 || request.Column >= game.Configuration.Width ||
                request.Row < 0 || request.Row >= game.Configuration.Height)
            {
                return Results.BadRequest("Invalid coordinates");
            }

            await GameService.Instance.ExecuteOnUI(() => {
                game.SuppressNotifications = !request.ShowMessage;
                var cell = game.Grid[request.Column, request.Row];
                if (request.Action == "flag") {
                     cell.Tile?.ToggleFlag();
                } else {
                    game.ClickCell(cell);
                }
            });
            return Results.Ok();
        });
    }
}
