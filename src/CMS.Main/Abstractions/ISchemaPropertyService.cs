using Ardalis.Result;
using CMS.Main.DTOs.SchemaProperty;

namespace CMS.Main.Abstractions;

public interface ISchemaPropertyService
{
    Task<Result<SchemaPropertyDto>> CreateSchemaPropertyAsync(
        SchemaPropertyDto dto);

    Task<Result<SchemaPropertyDto>> UpdateSchemaPropertyAsync(
        SchemaPropertyDto dto);
    
    Task<Result> DeleteSchemaPropertyAsync(
            string propertyId);
}