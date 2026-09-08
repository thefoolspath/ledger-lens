using LedgerLens.MarketData.Api;
using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddMarketDataApplication();
builder.AddMarketDataInfrastructure();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("market-api");
app.MapMarketDataEndpoints();
app.Run();
public partial class Program;
