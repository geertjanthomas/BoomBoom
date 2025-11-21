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
                        HasBomb = cell.Exposed ? (bool?)cell.HasBomb : null,
                        Adjacent = cell.Exposed ? (int?)cell.NumberOfAdjacentBombs : null
                    });
                }
            }
            
            return Results.Ok(new {
                Running = game.IsRunning,
                Paused = game.IsPaused,
                Won = game.Statistics.Win,
                Width = game.Configuration.Width,
                Height = game.Configuration.Height,
                Bombs = game.Configuration.NumberOfBombs,
                Grid = grid
            });
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
                var cell = game.Grid[request.Column, request.Row];
                if (request.Action == "flag") {
                     cell.Flagged = !cell.Flagged;
                } else {
                    game.ClickCell(cell);
                }
            });
            return Results.Ok();
        });
    }
}
