using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";
    private static readonly List<GameDto> games = [
        new (1, "Final Fantasy 1", "Roleplaying", 59.99M, new DateOnly(2010, 9, 13)),
        new (2, "Final Fantasy 2", "Roleplaying", 39.99M, new DateOnly(2013, 9, 12)),
        new (3, "Final Fantasy 3", "Roleplaying", 89.99M, new DateOnly(2010, 9, 10)),
        new (4, "Final Fantasy 4", "Roleplaying", 29.99M, new DateOnly(2011, 9, 14)),
    ];

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication app)
    {

        var group = app.MapGroup("games")
                        .WithParameterValidation();

        // GET /games
        group.MapGet("/", () => games);

        // GET /games/{id}
        group.MapGet("/{id}", (int id) =>
        {
            GameDto? game = games.Find(game => game.Id == id);

            return game is null ? Results.NotFound() : Results.Ok(game);
        })
        .WithName(GetGameEndpointName);

        // POST /games
        group.MapPost("/", (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = new()
            {
                Name = newGame.Name,
                Genre = dbContext.Genres.Find(newGame.GenreId),
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbContext.Games.Add(game);
            dbContext.SaveChanges();

            return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, game);
        });

        // PUT /games
        group.MapPut("/{id}", (int id, UpdateGameDto updatedGame) =>
        {
            var index = games.FindIndex(game => game.Id == id);

            if (index == -1) return Results.NotFound();

            games[index] = new GameDto(
                id,
                updatedGame.Name,
                updatedGame.Genre,
                updatedGame.Price,
                updatedGame.ReleaseDate
            );

            return Results.NoContent();
        });

        // DELETE /games
        group.MapDelete("/{id}", (int id) =>
        {
            games.RemoveAll((game) => game.Id == id);

            return Results.NoContent();
        });

        return group;
    }
}