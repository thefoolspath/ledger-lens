using LedgerLens.IdentityAccess.Api;
using LedgerLens.IdentityAccess.Application;
using LedgerLens.IdentityAccess.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddIdentityAccessApplication();
builder.AddIdentityAccessInfrastructure();
builder.AddLedgerLensApiResponses();
builder.Services.AddExceptionHandler<UnexpectedIdentityExceptionHandler>();
var app = builder.Build();
app.UseLedgerLensApiResponses();
app.UseExceptionHandler();
app.MapDefaultEndpoints("identity-api");
app.MapIdentityAccessEndpoints();
app.Run();
public partial class Program;
