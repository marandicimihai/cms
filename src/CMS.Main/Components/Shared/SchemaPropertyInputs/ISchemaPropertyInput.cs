namespace CMS.Main.Components.Shared.SchemaPropertyInputs;

public interface ISchemaPropertyInput
{
    object? GetValue();

    void SetDisabled(bool disabled);

    void Reset();
}
