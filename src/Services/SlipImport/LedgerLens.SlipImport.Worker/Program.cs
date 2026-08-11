using LedgerLens.SlipImport.Infrastructure;
using LedgerLens.SlipImport.Worker;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddSlipImportInfrastructure();
builder.Services.AddHostedService<FoundationWorker>();
var app = builder.Build();
app.MapDefaultEndpoints("slip-worker");
app.Run();
