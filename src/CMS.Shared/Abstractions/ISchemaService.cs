using Ardalis.Result;
using CMS.Shared.DTOs.Schema;

namespace CMS.Shared.Abstractions;

public interface ISchemaService
{
    Task<Result<SchemaDto>> GetSchemaByIdAsync(
        string schemaId,
        Action<SchemaGetOptions>? optionsAction = null);

    Task<Result<SchemaDto>> CreateSchemaAsync(
        SchemaDto dto);

    Task<Result> DeleteSchemaAsync(
        string schemaId);
}