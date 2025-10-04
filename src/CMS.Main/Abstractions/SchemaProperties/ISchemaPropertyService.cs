using Ardalis.Result;
using CMS.Main.DTOs;

namespace CMS.Main.Abstractions.SchemaProperties;

public interface ISchemaPropertyService
{
    Task<Result<SchemaPropertyDto>> CreateSchemaPropertyAsync(
        SchemaPropertyDto dto);

    Task<Result<SchemaPropertyDto>> UpdateSchemaPropertyAsync(
        SchemaPropertyDto dto);
    
    Task<Result> DeleteSchemaPropertyAsync(
            string propertyId);
}