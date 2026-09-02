using LedgerLens.MarketData.Api;
using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddMarketDataApplication();
builder.AddMarketDataInfrastructure();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("market-api");
app.MapMarketDataEndpoints();
app.Run();
public partial class Program;
