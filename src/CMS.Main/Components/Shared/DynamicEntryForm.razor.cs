using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Shared;

public partial class DynamicEntryForm : ComponentBase
{
    private static readonly object FormModel = new();

    [Parameter] public IEnumerable<SchemaPropertyWithIdDto>? Properties { get; set; }
    [Parameter] public EventCallback<DynamicEntryResult> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    // Single value bag for all dynamic fields
    protected Dictionary<string, object?> Values { get; } = new();

    protected override void OnParametersSet()
    {
        if (Properties == null) return;
        foreach (var prop in Properties)
        {
            if (!Values.ContainsKey(prop.Id))
            {
                Values[prop.Id] = prop.Type switch
                {
                    SchemaPropertyType.Boolean => false,
                    _ => null
                };
            }
        }
    }

    // Helpers
    protected string? GetString(string id) => Values.TryGetValue(id, out var v) ? v?.ToString() : null;
    protected void SetString(ChangeEventArgs e, string id) => Values[id] = e.Value?.ToString();

    protected bool GetBool(string id) => Values.TryGetValue(id, out var v) && v is bool b && b;
    protected void SetBool(ChangeEventArgs e, string id) => Values[id] = e.Value is bool b ? b : (e.Value?.ToString() == "true" || e.Value?.ToString() == "on");

    protected async Task HandleValidSubmit()
    {
        var snapshot = Values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        var result = new DynamicEntryResult(snapshot);
        if (OnSubmit.HasDelegate)
        {
            await OnSubmit.InvokeAsync(result);
        }
    }

    protected async Task HandleCancel()
    {
        if (OnCancel.HasDelegate)
        {
            await OnCancel.InvokeAsync();
        }
    }

    public record DynamicEntryResult(Dictionary<string, object?> Values);
}
