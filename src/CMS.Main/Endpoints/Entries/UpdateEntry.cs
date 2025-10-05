using System.Text.Json.Serialization;
using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Auth;
using CMS.Main.DTOs;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Endpoints.Entries;

public class UpdateEntryRequest
{
    [RouteParam]
    public string EntryId { get; set; } = string.Empty;

    [FromBody]
    [JsonConverter(typeof(Serialization.DictionaryStringJsonConverter))]
    public Dictionary<string, string?>? Fields { get; set; }

    internal sealed class UpdateEntryRequestValidator : Validator<UpdateEntryRequest>
    {
        public UpdateEntryRequestValidator()
        {
            RuleFor(x => x.EntryId)
                .NotEmpty()
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("Property 'EntryId' must be a valid GUID.");

            RuleFor(x => x.Fields)
                .NotNull()
                .WithMessage("Property 'RawProperties' must be a not null json object.");
        }
    }
}

public class UpdateEntry(
    IAuthorizationService authService,
    IEntryService entryService
) : Endpoint<UpdateEntryRequest>
{
    public override void Configure()
    {
        Put("entry/{entryId}");
        Group<EntriesGroup>();
    }

    public override async Task HandleAsync(UpdateEntryRequest req, CancellationToken ct)
    {
        var authResult = await authService.AuthorizeAsync(User, req.EntryId, AuthConstants.CanEditEntry);
        if (!authResult.Succeeded)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var dto = new EntryDto
        {
            Id = req.EntryId,
            Fields = req.Fields
                ?.ToDictionary(kvp => kvp.Key, object? (kvp) => kvp.Value)
                ?? []
        };

        var result = await entryService.UpdateEntryAsync(dto);
        if (result.IsNotFound())
        {
            await Send.NotFoundAsync(ct);
        }
        else if (result.Errors.Any())
        {
            foreach (var error in result.Errors)
            {
                AddError(error);
            }
            ThrowIfAnyErrors();
        }
        else if (result.ValidationErrors.Any())
        {
            foreach (var error in result.ValidationErrors)
            {
                AddError(error.ErrorMessage);
            }
            ThrowIfAnyErrors();
        }
        else if (!result.IsSuccess)
        {
            ThrowError("There was an error.");
        }
    }
}
