using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Api.Features.Portfolios.GetOne;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.Portfolios.GetOne;
using LedgerLens.PortfolioCore.Domain;
using LedgerLens.ServiceDefaults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LedgerLens.PortfolioCore.IntegrationTests;

public sealed class UuidResourceInputTests
{
    [Fact]
    public async Task Unknown_uuidv4_reaches_portfolio_lookup_and_returns_not_found()
    {
        var id = Guid.Parse("019a0f10-0000-4000-8000-000000000001");
        var lookup = new MissingPortfolioLookup();
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.AddLedgerLensApiResponses();
        builder.Services.AddSingleton<IQueryHandler<PortfoliosGetOneQuery, PortfolioOverview?>>(lookup);
        await using var app = builder.Build();
        app.UseLedgerLensApiResponses();
        app.MapGroup("/v1").MapPortfoliosGetOne();
        await app.StartAsync();

        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
        using var response = await client.GetAsync($"/v1/portfolios/get-one/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ApiErrorCodes.ResourceNotFound,
            (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        Assert.Equal(id, lookup.SeenId);

        using var malformed = await client.GetAsync("/v1/portfolios/get-one/not-a-guid");
        Assert.Equal(HttpStatusCode.NotFound, malformed.StatusCode);
        Assert.Equal(id, lookup.SeenId);
    }

    [Fact]
    public void LedgerLens_generated_identifier_remains_uuidv7()
    {
        var id = DomainId.New();
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(7, id.Version);
    }

    private sealed class MissingPortfolioLookup : IQueryHandler<PortfoliosGetOneQuery, PortfolioOverview?>
    {
        public Guid? SeenId { get; private set; }

        public Task<PortfolioOverview?> HandleAsync(PortfoliosGetOneQuery query, CancellationToken cancellationToken)
        {
            SeenId = query.PortfolioId;
            return Task.FromResult<PortfolioOverview?>(null);
        }
    }
}
