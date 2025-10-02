using CMS.Main.Abstractions.SchemaProperties.SchemaPropertyTypes;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.Abstractions.SchemaProperties;

public class SchemaPropertyTypeHandlerFactory : ISchemaPropertyTypeHandlerFactory
{
    private readonly List<ISchemaPropertyTypeHandler> handlers;

    public SchemaPropertyTypeHandlerFactory()
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

    public ISchemaPropertyTypeHandler GetHandler(SchemaPropertyType propertyType)
    {
        if (handlers.FirstOrDefault(h => h.TypeName == propertyType) is ISchemaPropertyTypeHandler handler)
        {
            return handler;
        }

        throw new NotSupportedException($"Schema property type '{propertyType}' is not supported.");
    }
}