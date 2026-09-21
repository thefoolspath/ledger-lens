using System.Globalization;
using LedgerLens.ServiceDefaults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class ApiResponseExtensions
{
    private static readonly CultureInfo[] SupportedCultures =
    [
        CultureInfo.GetCultureInfo("en-US"),
        CultureInfo.GetCultureInfo("th-TH"),
    ];

    public static TBuilder AddLedgerLensApiResponses<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(SupportedCultures[0]);
            options.SupportedCultures = SupportedCultures;
            options.SupportedUICultures = SupportedCultures;
        });
        builder.Services.AddSingleton<ApiProblemFactory>();
        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
                context.HttpContext.RequestServices.GetRequiredService<ApiProblemFactory>()
                    .Customize(context.ProblemDetails, context.HttpContext);
        });
        return builder;
    }

    public static WebApplication UseLedgerLensApiResponses(this WebApplication app)
    {
        app.UseRequestLocalization();
        return app;
    }
}
