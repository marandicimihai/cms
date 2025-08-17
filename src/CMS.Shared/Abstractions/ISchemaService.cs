using Ardalis.Result;
using CMS.Shared.DTOs.Schema;

namespace CMS.Shared.Abstractions;

public interface ISchemaService
{
    Task<Result<SchemaWithIdDto>> GetSchemaByIdAsync(
        string schemaId,
        Action<SchemaGetOptions>? optionsAction = null);

    Task<Result<SchemaWithIdDto>> CreateSchemaAsync(
        SchemaCreationDto schemaCreationDto);

    Task<Result> DeleteSchemaAsync(
        string schemaId);
}