using System.Text.Json.Serialization;
using Ardalis.Result;
using CMS.Main.Abstractions;
using CMS.Main.DTOs.Entry;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Endpoints.Entries;

public class CreateEntryRequest
{
    [RouteParam]
    public string SchemaId { get; set; } = default!;

    [FromBody]
    [JsonConverter(typeof(Serialization.DictionaryStringJsonConverter))]
    public Dictionary<string, string?>? RawProperties { get; set; }
    
    internal sealed class CreateEntryRequestValidator : Validator<CreateEntryRequest>
    {
        public CreateEntryRequestValidator()
        {
            RuleFor(x => x.SchemaId)
                .NotEmpty()
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("Property 'SchemaId' must be a valid GUID.");

            RuleFor(x => x.RawProperties)
                .NotNull()
                .WithMessage("Property 'RawProperties' must be a not null json object.");
        }
    }
}

public class CreateEntry(
    IAuthorizationService authService,
    IEntryService entryService
) : Endpoint<CreateEntryRequest>
{
    public override void Configure()
    {
        Post("/");
        Group<EntriesGroup>();
    }

    public override async Task HandleAsync(CreateEntryRequest req, CancellationToken ct)
    {
        var authResult = await authService.AuthorizeAsync(User, req.SchemaId, "SchemaPolicies.CanEditSchema");
        if (!authResult.Succeeded)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        
        var creationDto = new EntryDto
        {
            SchemaId = req.SchemaId,
            Fields = req.RawProperties
                ?.ToDictionary(kvp => kvp.Key, object? (kvp) => kvp.Value)
                ?? new Dictionary<string, object?>()
        };

        var result = await entryService.AddEntryAsync(creationDto);
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