using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.DTOs.Entry;
using CMS.Main.DTOs.Pagination;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Endpoints.Entries;

public class GetEntriesRequest
{
    [RouteParam]
    public string SchemaId { get; set; } = string.Empty;

    [QueryParam]
    public int? PageNumber { get; set; }
    
    [QueryParam]
    public int? PageSize { get; set; }
    
    internal sealed class GetEntriesRequestValidator : Validator<GetEntriesRequest>
    {
        public GetEntriesRequestValidator()
        {
            RuleFor(x => x.SchemaId)
                .NotEmpty()
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("Property 'SchemaId' must be a valid GUID.");
            
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .When(x => x.PageNumber.HasValue)
                .WithMessage("Property 'PageNumber' must be greater than or equal to 1.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .When(x => x.PageSize.HasValue)
                .WithMessage("Property 'PageSize' must be greater than or equal to 1.");
        }
    }
}

public class GetEntriesResponse
{
    public List<EntryDto> Entries { get; set; } = [];
    public PaginationMetadata PaginationMetadata { get; set; } = default!;
}

public class GetEntries(
    IEntryService entryService, 
    IAuthorizationService authService
) : Endpoint<GetEntriesRequest, GetEntriesResponse>
{
    public override void Configure()
    {
        Get("{schemaId}/entries");
        Group<EntriesGroup>();
    }

    public override async Task HandleAsync(GetEntriesRequest req, CancellationToken ct)
    {
        var authResult = await authService.AuthorizeAsync(User, req.SchemaId, "SchemaPolicies.CanEditSchema");
        if (!authResult.Succeeded)
        {
            // Don't hint if the schema exists
            await Send.NotFoundAsync(ct);
            return;
        }
        
        var result = await entryService.GetEntriesForSchema(
            req.SchemaId,
            new(req.PageNumber ?? 1, req.PageSize ?? 10));
        
        if (result.IsSuccess)
        {
            var (entries, pagination) = result.Value;
            Response = new()
            {
                Entries = entries,
                PaginationMetadata = pagination
            };
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
        else
        {
            ThrowError("There was an error.");
        }
    }
}