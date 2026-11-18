using ATM.Api;
using ATM.Application.Commands;
using ATM.Application.Persistence;
using ATM.Application.Queries;
using ATM.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddAtmPersistence();
builder.Services.AddSingleton<AtmOperationGate>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<AtmCommandHandler>();
builder.Services.AddScoped<DashboardQueryHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapAtmEndpoints();
await app.Services.InitializeAtmDatabaseAsync();

app.Run();
