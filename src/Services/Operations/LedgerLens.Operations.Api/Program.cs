using LedgerLens.Operations.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddOperationsInfrastructure();
builder.AddLedgerLensApiResponses();
var app = builder.Build();
app.UseLedgerLensApiResponses();
app.UseExceptionHandler();
app.MapDefaultEndpoints("operations-api");
app.Run();
public partial class Program;
