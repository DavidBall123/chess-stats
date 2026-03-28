var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "api",
    status = "ok",
    utc = DateTimeOffset.UtcNow
}));

app.MapGet("/health", () => Results.Ok(new
{
    service = "api",
    healthy = true,
    utc = DateTimeOffset.UtcNow
}));

app.Run();
