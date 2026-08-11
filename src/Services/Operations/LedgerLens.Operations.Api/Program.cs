using LedgerLens.Operations.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddOperationsInfrastructure();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("operations-api");
app.Run();
public partial class Program;
