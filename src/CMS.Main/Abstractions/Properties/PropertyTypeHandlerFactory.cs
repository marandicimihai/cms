using CMS.Main.Abstractions.Properties.PropertyTypes;

namespace CMS.Main.Abstractions.SchemaProperties;

public class PropertyTypeHandlerFactory : IPropertyTypeHandlerFactory
{
    private readonly List<IPropertyTypeHandler> handlers;

    public PropertyTypeHandlerFactory()
    {
        handlers =
        [
            new TextPropertyType(),
            new BooleanPropertyType(),
            new DateTimePropertyType(),
            new NumberPropertyType(),
            new EnumPropertyType()
        ];
    }

    public IPropertyTypeHandler GetHandler(PropertyType propertyType)
    {
        if (handlers.FirstOrDefault(h => h.TypeName == propertyType) is IPropertyTypeHandler handler)
        {
            return handler;
        }

        throw new NotSupportedException($"Schema property type '{propertyType}' is not supported.");
    }
}