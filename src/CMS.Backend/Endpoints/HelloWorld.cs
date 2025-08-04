using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CMS.Backend.Endpoints;

public class HelloWorld : EndpointWithoutRequest<Ok<string>>
{
    public override void Configure()
    {
        Get("/");
        AllowAnonymous();
    }

    public override async Task<Ok<string>> ExecuteAsync(CancellationToken ct)
    {
        await Task.CompletedTask;
        return TypedResults.Ok("Hello, World!");
    }
}