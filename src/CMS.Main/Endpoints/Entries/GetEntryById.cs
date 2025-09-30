using Ardalis.Result;
using CMS.Main.Abstractions.Entries;
using CMS.Main.Auth;
using CMS.Main.DTOs.Entry;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Endpoints.Entries;

public class GetEntryByIdRequest
{
    [RouteParam]
    public string EntryId { get; set; } = string.Empty;

    internal sealed class GetEntryByIdRequestValidator : Validator<GetEntryByIdRequest>
    {
        public GetEntryByIdRequestValidator()
        {
            RuleFor(x => x.EntryId)
                .NotEmpty()
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("Property 'EntryId' must be a valid GUID.");
        }
    }
}

public class GetEntryById(
    IAuthorizationService authService,
    IEntryService entryService
) : Endpoint<GetEntryByIdRequest, EntryDto>
{
    public override void Configure()
    {
        Get("entry/{entryId}");
        Group<EntriesGroup>();
    }

    public override async Task HandleAsync(GetEntryByIdRequest req, CancellationToken ct)
    {
        var authResult = await authService.AuthorizeAsync(User, req.EntryId, AuthConstants.CanEditEntry);
        if (!authResult.Succeeded)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = await entryService.GetEntryByIdAsync(req.EntryId);
        if (result.IsSuccess)
        {
            Response = result.Value;
        }
        else if (result.IsNotFound())
        {
            await Send.NotFoundAsync(ct);
        }
        else
        {
            ThrowError("There was an error.");
        }
    }
}
