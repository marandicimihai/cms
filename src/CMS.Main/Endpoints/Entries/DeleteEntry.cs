using Ardalis.Result;
using CMS.Main.Abstractions;
using CMS.Main.Auth;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace CMS.Main.Endpoints.Entries;

public class DeleteEntryRequest
{
    [RouteParam]
    public string EntryId { get; set; } = string.Empty;

    internal sealed class DeleteEntryRequestValidator : Validator<DeleteEntryRequest>
    {
        public DeleteEntryRequestValidator()
        {
            RuleFor(x => x.EntryId)
                .NotEmpty()
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("Property 'EntryId' must be a valid GUID.");
        }
    }
}

public class DeleteEntry(
    IAuthorizationService authService,
    IEntryService entryService
) : Endpoint<DeleteEntryRequest>
{
    public override void Configure()
    {
        Delete("entry/{entryId}");
        Group<EntriesGroup>();
    }

    public override async Task HandleAsync(DeleteEntryRequest req, CancellationToken ct)
    {
        var authResult = await authService.AuthorizeAsync(User, req.EntryId, AuthConstants.CanEditEntry);
        if (!authResult.Succeeded)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = await entryService.DeleteEntryAsync(req.EntryId);
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
        else if (!result.IsSuccess)
        {
            ThrowError("There was an error.");
        }
    }
}
