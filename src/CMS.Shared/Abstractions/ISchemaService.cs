using Ardalis.Result;
using CMS.Shared.DTOs.Schema;

namespace CMS.Shared.Abstractions;

public interface ISchemaService
{
     Task<Result<SchemaWithIdDto>> CreateSchemaAsync(
        SchemaCreationDto schemaCreationDto);
}