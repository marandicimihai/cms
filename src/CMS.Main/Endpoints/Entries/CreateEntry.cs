using System.Text.Json.Serialization;
using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Auth;
using CMS.Main.DTOs;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Endpoints.Entries;

public class CreateEntryRequest
{
    [RouteParam]
    public string SchemaId { get; set; } = string.Empty;

    [FromBody]
    [JsonConverter(typeof(Serialization.DictionaryStringJsonConverter))]
    public Dictionary<string, string?>? Fields { get; set; }

    internal sealed class CreateEntryRequestValidator : Validator<CreateEntryRequest>
    {
        public CreateEntryRequestValidator()
        {
            RuleFor(x => x.SchemaId)
                .NotEmpty()
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("Property 'SchemaId' must be a valid GUID.");

            RuleFor(x => x.Fields)
                .NotNull()
                .WithMessage("Property 'RawProperties' must be a not null json object.");
        }
    }
}

public class CreateEntryResponse
{
    [FromBody]
    public EntryDto Entry { get; set; } = default!;
}

public class CreateEntry(
    IAuthorizationService authService,
    IEntryService entryService
) : Endpoint<CreateEntryRequest, CreateEntryResponse>
{
    public override void Configure()
    {
        Post("{schemaId}/entries");
        Group<EntriesGroup>();
        Description(b => b
            .Produces<CreateEntryResponse>(201));
    }

    public override async Task HandleAsync(CreateEntryRequest req, CancellationToken ct)
    {
        var authResult = await authService.AuthorizeAsync(User, req.SchemaId, AuthConstants.OwnsSchema);
        if (!authResult.Succeeded)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var creationDto = new EntryDto
        {
            SchemaId = req.SchemaId,
            Fields = req.Fields
                ?.ToDictionary(kvp => kvp.Key, object? (kvp) => kvp.Value)
                ?? []
        };

        var result = await entryService.AddEntryAsync(creationDto);
        if (result.IsSuccess)
        {
            Response.Entry = result.Value!;
            await Send.CreatedAtAsync<GetEntryById>(new
            {
                entryId = result.Value!.Id
            }, cancellation: ct);
        }
        else if (result.IsNotFound())
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
        else
        {
            ThrowError("There was an error.");
        }
    }
}