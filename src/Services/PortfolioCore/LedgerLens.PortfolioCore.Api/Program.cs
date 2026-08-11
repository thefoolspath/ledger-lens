using LedgerLens.PortfolioCore.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddPortfolioCoreInfrastructure();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("portfolio-api");
app.Run();
public partial class Program;
