using ChessMonitor.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<HeartbeatWorker>();

var app = builder.Build();
app.Run();
