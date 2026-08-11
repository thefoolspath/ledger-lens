using LedgerLens.SlipImport.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddSlipImportInfrastructure();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("slip-api");
app.Run();
public partial class Program;
