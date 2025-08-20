using Ardalis.Result;
using CMS.Shared.DTOs.SchemaProperty;

namespace CMS.Shared.Abstractions;

public interface ISchemaPropertyService
{
    Task<Result<SchemaPropertyDto>> CreateSchemaPropertyAsync(
        SchemaPropertyDto dto);

    Task<Result<SchemaPropertyDto>> UpdateSchemaPropertyAsync(
        SchemaPropertyDto dto);
    
    Task<Result> DeleteSchemaPropertyAsync(
            string propertyId);
}