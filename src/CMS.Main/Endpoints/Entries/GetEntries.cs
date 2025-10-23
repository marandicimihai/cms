using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Auth;
using CMS.Main.DTOs;
using CMS.Main.DTOs.Pagination;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using static CMS.Main.Endpoints.Entries.GetEntriesFilter;

namespace CMS.Main.Endpoints.Entries;

public class GetEntriesRequest
{
    [RouteParam]
    public string SchemaId { get; set; } = string.Empty;

    [QueryParam]
    public int? PageNumber { get; set; }

    [QueryParam]
    public int? PageSize { get; set; }

    [FromBody]
    public GetEntriesRequestBody? Body { get; set; } = new();

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

            RuleFor(x => x.Body!.SortByPropertyName)
                .MaximumLength(50)
                .WithMessage("Property 'SortByPropertyName' must be at most 50 characters long.");

            RuleForEach(x => x.Body!.Filters)
                .SetValidator(new GetEntriesFilterValidator());
        }
    }
}


public class GetEntriesRequestBody
{
    public string? SortByPropertyName { get; set; }
    public bool Descending { get; set; } = false;

    public List<GetEntriesFilter> Filters { get; set; } = [];
}

public class GetEntriesFilter
{
    public string PropertyName { get; set; } = string.Empty;
    public string FilterType { get; set; } = string.Empty;
    public object? ReferenceValue { get; set; } = string.Empty;

    public class GetEntriesFilterValidator : Validator<GetEntriesFilter>
    {
        public GetEntriesFilterValidator()
        {
            RuleFor(x => x.PropertyName)
                .MaximumLength(50)
                .WithMessage("Property 'PropertyName' must be at most 50 characters long.");

            RuleFor(x => x.FilterType)
                .Must(x => Enum.TryParse(typeof(PropertyFilter), x, true, out _))
                .WithMessage("Property 'FilterType' must be a valid filter type.");
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
        var authResult = await authService.AuthorizeAsync(User, req.SchemaId, AuthConstants.OwnsSchema);
        if (!authResult.Succeeded)
        {
            // Don't hint if the schema exists
            await Send.NotFoundAsync(ct);
            return;
        }
        var result = await entryService.GetEntriesForSchema(
            req.SchemaId,
            new(req.PageNumber ?? 1, req.PageSize ?? 10),
            opt =>
            {
                if (!string.IsNullOrEmpty(req.Body?.SortByPropertyName))
                {
                    opt.SortByPropertyName = req.Body!.SortByPropertyName;
                    opt.Descending = req.Body!.Descending;
                }
                if (req.Body?.Filters is not null && req.Body.Filters.Count != 0)
                {
                    opt.Filters = req.Body.Filters
                        .Select(f =>
                        {
                            if (Enum.TryParse<PropertyFilter>(f.FilterType, true, out var filterType))
                            {
                                return new EntryFilter()
                                {
                                    PropertyName = f.PropertyName,
                                    FilterType = filterType,
                                    ReferenceValue = f.ReferenceValue?.ToString()
                                };
                            }
                            return null;
                        })
                        .Where(e => e is not null)
                        .Cast<EntryFilter>()
                        .ToList();
                }
            });
        
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
        else if (result.ValidationErrors.Any())
        {
            foreach (var error in result.ValidationErrors)
            {
                AddError(error.ErrorMessage);
            }
            ThrowIfAnyErrors();
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