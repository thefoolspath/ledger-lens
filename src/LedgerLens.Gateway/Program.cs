var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddLedgerLensApiResponses();
builder.Services.AddReverseProxy()
    .LoadFromMemory(GatewayRoutes.Routes, GatewayRoutes.Clusters)
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();
app.UseLedgerLensApiResponses();
app.UseExceptionHandler();
app.MapDefaultEndpoints("gateway");
app.MapReverseProxy();
app.Run();

public partial class Program;
