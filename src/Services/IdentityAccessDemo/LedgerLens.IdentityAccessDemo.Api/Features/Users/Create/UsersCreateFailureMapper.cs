using LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;
using LedgerLens.ApiContracts;
using LedgerLens.ServiceDefaults;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace LedgerLens.IdentityAccessDemo.Api.Features.Users.Create;

public static class UsersCreateFailureMapper
{
    private const string EmailExistsCode = "identity_demo.users.email_already_exists";
    private const string RequestIdConflictCode = "identity_demo.users.request_id_conflict";

    public static IResult Map(UsersCreateFailure failure, HttpContext context, ApiProblemFactory problems,
        IStringLocalizer<IdentityDemoMessages> messages, ILogger<IdentityDemoMessages> logger) => failure switch
    {
        UsersCreateFailure.InvalidRequest => problems.Validation(context, ApiErrorCodes.InvalidRequest),
        UsersCreateFailure.EmailAlreadyExists => Conflict(EmailExistsCode, context, problems, messages, logger),
        UsersCreateFailure.RequestIdConflict => Conflict(RequestIdConflictCode, context, problems, messages, logger),
        _ => throw new InvalidOperationException("Unknown UsersCreate failure."),
    };

    private static IResult Conflict(string code, HttpContext context, ApiProblemFactory problems,
        IStringLocalizer<IdentityDemoMessages> messages, ILogger<IdentityDemoMessages> logger)
    {
        return problems.Conflict(context, code,
            Text($"{code}.Title", "State conflict", messages, logger),
            Text($"{code}.Detail", "The operation conflicts with the resource's current state.", messages, logger));
    }

    private static string Text(string key, string fallback, IStringLocalizer<IdentityDemoMessages> messages,
        ILogger<IdentityDemoMessages> logger)
    {
        var value = messages[key];
        if (!value.ResourceNotFound)
            return value.Value;
        logger.LogWarning("Demo API message resource {ResourceKey} was not found for culture {Culture}",
            key, CultureInfo.CurrentUICulture.Name);
        return fallback;
    }
}
