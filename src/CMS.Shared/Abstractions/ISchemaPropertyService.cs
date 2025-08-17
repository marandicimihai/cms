using Ardalis.Result;
using CMS.Shared.DTOs.SchemaProperty;

namespace CMS.Shared.Abstractions;

public interface ISchemaPropertyService
{
    Task<Result<SchemaPropertyWithIdDto>> CreateSchemaPropertyAsync(
        SchemaPropertyCreationDto creationDto);
    
    Task<Result> DeleteSchemaPropertyAsync(
            string propertyId);
}