using Ardalis.Result;
using CMS.Main.DTOs;

namespace CMS.Main.Abstractions.SchemaProperties;

public interface IPropertyService
{
    Task<Result<PropertyDto>> CreatePropertyAsync(
        PropertyDto dto);

    Task<Result<PropertyDto>> UpdatePropertyAsync(
        PropertyDto dto);

    Task<Result> DeletePropertyAsync(
        string propertyId);
}