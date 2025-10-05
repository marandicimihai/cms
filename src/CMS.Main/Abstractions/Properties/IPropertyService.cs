using Ardalis.Result;
using CMS.Main.DTOs;

namespace CMS.Main.Abstractions.SchemaProperties;

public interface ISchemaPropertyService
{
    Task<Result<PropertyDto>> CreateSchemaPropertyAsync(
        PropertyDto dto);

    Task<Result<PropertyDto>> UpdateSchemaPropertyAsync(
        PropertyDto dto);
    
    Task<Result> DeleteSchemaPropertyAsync(
            string propertyId);
}