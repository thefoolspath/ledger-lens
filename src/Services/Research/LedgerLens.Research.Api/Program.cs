using LedgerLens.Research.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddResearchInfrastructure();
builder.AddLedgerLensApiResponses();
var app = builder.Build();
app.UseLedgerLensApiResponses();
app.UseExceptionHandler();
app.MapDefaultEndpoints("research-api");
app.Run();
public partial class Program;
