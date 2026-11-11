using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 65536);
var app = builder.Build();
var host = Environment.GetEnvironmentVariable("HOST") ?? "127.0.0.1";
var port = Environment.GetEnvironmentVariable("PORT") ?? throw new InvalidOperationException("PORT is required");
var path = Environment.GetEnvironmentVariable("OPERATION_PATH") ?? throw new InvalidOperationException("OPERATION_PATH is required");

app.Urls.Add($"http://{host}:{port}");
app.MapGet("/health", () => new { status = "up" });
app.MapPost(path, (JsonElement input) =>
{
    try
    {
        return Results.Ok(Domain.Execute(input));
    }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or KeyNotFoundException or FormatException)
    {
        return Results.BadRequest(new { error = error.Message });
    }
});
app.Run();
