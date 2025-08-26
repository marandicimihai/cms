using Ardalis.Result;
using CMS.Main.DTOs.Schema;

namespace CMS.Main.Abstractions;

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