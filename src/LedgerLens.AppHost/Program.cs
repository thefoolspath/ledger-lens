using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);

var usePersistentVolumes = !bool.TryParse(
    builder.Configuration["LedgerLens:Runtime:UsePersistentVolumes"],
    out var configuredUsePersistentVolumes) || configuredUsePersistentVolumes;
var postgresPassword = usePersistentVolumes
    ? builder.AddParameter("postgres-password", secret: true)
    : builder.AddParameter("postgres-password", new GenerateParameterDefault(), secret: true);
var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithImage("postgres", "18.4");

var portfolioDatabase = postgres.AddDatabase("portfolio-db", "ledgerlens_portfolio");
var marketDatabase = postgres.AddDatabase("market-db", "ledgerlens_market");
var slipDatabase = postgres.AddDatabase("slip-db", "ledgerlens_slip");
var researchDatabase = postgres.AddDatabase("research-db", "ledgerlens_research");
var operationsDatabase = postgres.AddDatabase("operations-db", "ledgerlens_operations");

var messaging = builder.AddNats("messaging")
    .WithJetStream();

if (usePersistentVolumes)
{
    postgres.WithDataVolume("ledgerlens-postgres-data");
    messaging.WithDataVolume("ledgerlens-nats-data");
}

var portfolio = builder.AddProject<Projects.LedgerLens_PortfolioCore_Api>("portfolio-api")
    .WithReference(portfolioDatabase).WithReference(messaging)
    .WaitFor(portfolioDatabase).WaitFor(messaging)
    .WithHttpHealthCheck("/health");
var market = builder.AddProject<Projects.LedgerLens_MarketData_Api>("market-api")
    .WithReference(marketDatabase).WithReference(messaging)
    .WaitFor(marketDatabase).WaitFor(messaging)
    .WithHttpHealthCheck("/health");
var slips = builder.AddProject<Projects.LedgerLens_SlipImport_Api>("slip-api")
    .WithReference(slipDatabase).WithReference(messaging)
    .WaitFor(slipDatabase).WaitFor(messaging)
    .WithHttpHealthCheck("/health");
var research = builder.AddProject<Projects.LedgerLens_Research_Api>("research-api")
    .WithReference(researchDatabase).WithReference(messaging)
    .WaitFor(researchDatabase).WaitFor(messaging)
    .WithHttpHealthCheck("/health");
var operations = builder.AddProject<Projects.LedgerLens_Operations_Api>("operations-api")
    .WithReference(operationsDatabase).WithReference(messaging)
    .WaitFor(operationsDatabase).WaitFor(messaging)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.LedgerLens_SlipImport_Worker>("slip-worker")
    .WithReference(slipDatabase).WithReference(messaging)
    .WaitFor(slipDatabase).WaitFor(messaging)
    .WithHttpHealthCheck("/health");

var gateway = builder.AddProject<Projects.LedgerLens_Gateway>("gateway")
    .WithExternalHttpEndpoints()
    .WithReference(portfolio).WithReference(market).WithReference(slips)
    .WithReference(research).WithReference(operations)
    .WithHttpHealthCheck("/health");

builder.AddJavaScriptApp("web", "../../web/ledgerlens-web")
    .WithRunScript("start")
    .WithHttpEndpoint(targetPort: 4200, name: "http")
    .WithReference(gateway)
    .WaitFor(gateway)
    .WithHttpHealthCheck("/")
    .WithExternalHttpEndpoints();

builder.Build().Run();
