using HookHubNet.Common.DTOs;

// Create the web application builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the dependency injection container
builder.Services.AddControllers();
builder.Services.AddSingleton<TunnelRegistry>();
//builder.Services.AddHostedService<TcpTunnelListener>();

// Build the web application
var app = builder.Build();

// Configure the HTTP request pipeline
app.UseWebSockets();
app.MapControllers();

// Run the application
app.Run();
