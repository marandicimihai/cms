namespace CMS.Main.Components.Shared.PropertyInputs;

public interface IPropertyInput
{
    object? GetValue();

    void SetDisabled(bool disabled);

    void Reset();
}
