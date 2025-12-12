using Npgsql;

namespace WebProj;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        // Simple health endpoint
        app.MapGet("/health", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connStr))
            {
                return Results.Problem("No connection string configured", statusCode: 500);
            }

            var maxAttempts = 5;
            var delay = TimeSpan.FromSeconds(2);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connStr);
                    await conn.OpenAsync();
                    await conn.CloseAsync();
                    return Results.Ok(new { status = "Healthy", db = "reachable" });
                }
                catch (Exception) when (attempt < maxAttempts)
                {
                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Database unreachable: {ex.Message}", statusCode: 500);
                }
            }

            return Results.Problem("Database unreachable after retries", statusCode: 500);
        });

        app.Run();
    }
}