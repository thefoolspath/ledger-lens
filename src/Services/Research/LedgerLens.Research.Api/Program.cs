using LedgerLens.Research.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddResearchInfrastructure();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("research-api");
app.Run();
public partial class Program;
