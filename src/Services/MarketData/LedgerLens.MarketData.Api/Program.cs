using LedgerLens.MarketData.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddMarketDataInfrastructure();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("market-api");
app.Run();
public partial class Program;
