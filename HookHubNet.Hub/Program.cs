using HookHubNet.Common.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<TunnelRegistry>();
//builder.Services.AddHostedService<TcpTunnelListener>();

var app = builder.Build();

app.UseWebSockets();
app.MapControllers();

app.Run();
