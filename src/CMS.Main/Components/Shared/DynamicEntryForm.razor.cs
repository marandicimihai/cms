using CMS.Shared.DTOs.SchemaProperty;
using Microsoft.AspNetCore.Components;

namespace CMS.Main.Components.Shared;

public partial class DynamicEntryForm : ComponentBase
{
    private object Model { get; set; } = new();

    [Parameter]
    public List<SchemaPropertyWithIdDto> Properties { get; set; } = [];
    
    [Parameter] 
    public EventCallback<Dictionary<SchemaPropertyWithIdDto, object?>> OnValidSubmit { get; set; }
    
    [Parameter] 
    public EventCallback OnCancel { get; set; }
    
    private List<DynamicEntryField?> Fields { get; set; } = [];

    protected override void OnParametersSet()
    {
        if (Fields.Count != Properties.Count)
        {
            Fields = Enumerable.Repeat<DynamicEntryField?>(null, Properties.Count).ToList();
        }
    }

    public void Reset()
    {
        foreach (var field in Fields)
        {
            field?.Reset();
        }
    }
    
    protected async Task HandleValidSubmit()
    {
        var isValid = true;
        foreach (var field in Fields)
        {
            var isFieldValid = field?.IsValid();

            if (isFieldValid is false or null)
                isValid = false;
        }
        
        if (!isValid)
            return;
        
        var propertiesAndValues = new Dictionary<SchemaPropertyWithIdDto, object?>();
        
        foreach (var field in Fields)
        {
            if (field is null) continue;
            
            var (property, value) = field.GetPropertyAndValue();
            propertiesAndValues.Add(property, value);
        }

        if (OnValidSubmit.HasDelegate)
        {
            await OnValidSubmit.InvokeAsync(propertiesAndValues);
        }
    }

    private async Task HandleCancel()
    {
        if (OnCancel.HasDelegate)
        {
            await OnCancel.InvokeAsync();
        }
    }
}
