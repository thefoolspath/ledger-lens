using LedgerLens.IdentityAccessDemo.Api;
using LedgerLens.IdentityAccessDemo.Application;
using LedgerLens.IdentityAccessDemo.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddIdentityAccessDemoApplication();
builder.AddIdentityAccessDemoInfrastructure();
builder.AddLedgerLensApiResponses();
builder.Services.AddExceptionHandler<UnexpectedIdentityExceptionHandler>();
var app = builder.Build();
app.UseLedgerLensApiResponses();
app.UseExceptionHandler();
app.MapDefaultEndpoints("identity-demo-api");
app.MapIdentityAccessDemoEndpoints();
app.Run();
public partial class Program;
