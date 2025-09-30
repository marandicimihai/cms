using Ardalis.Result;
using CMS.Main.DTOs.ApiKey;

namespace CMS.Main.Abstractions;

public interface IApiKeyService
{
    Task<Result<(string, ApiKeyDto)>> CreateApiKeyAsync(ApiKeyDto dto);
    
    Task<Result> UpdateApiKeyAsync(ApiKeyDto dto);
    
    Task<Result> DeleteApiKeyAsync(string apiKeyId);
}