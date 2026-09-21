using LedgerLens.SlipImport.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddSlipImportInfrastructure();
builder.AddLedgerLensApiResponses();
var app = builder.Build();
app.UseLedgerLensApiResponses();
app.UseExceptionHandler();
app.MapDefaultEndpoints("slip-api");
app.Run();
public partial class Program;
