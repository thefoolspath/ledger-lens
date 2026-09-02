using LedgerLens.PortfolioCore.Api;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddPortfolioCoreApplication();
builder.AddPortfolioCoreInfrastructure();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("portfolio-api");
app.MapPortfolioCoreEndpoints();
app.Run();
public partial class Program;
