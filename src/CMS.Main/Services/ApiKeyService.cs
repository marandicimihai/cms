using System.Security.Cryptography;
using Ardalis.Result;
using CMS.Main.Abstractions;
using CMS.Main.Data;
using CMS.Main.DTOs.ApiKey;
using CMS.Main.Models;
using Mapster;

namespace CMS.Main.Services;

public class ApiKeyService(
    IDbContextConcurrencyHelper dbHelper,
    ILogger<ApiKeyService> logger
) : IApiKeyService
{
    public async Task<Result<(string, ApiKeyDto)>> CreateApiKeyAsync(ApiKeyDto dto)
    {
        try
        {
            var newKey = dto.Adapt<ApiKey>();
            var keyValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var hashedKey = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(keyValue)));
            newKey.HashedKey = hashedKey;

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                try
                {
                    dbContext.ApiKeys.Add(newKey);
                    await dbContext.SaveChangesAsync();
                }
                catch
                {
                    dbContext.Entry(newKey).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    throw;
                }
            });

            return Result.Success((keyValue, newKey.Adapt<ApiKeyDto>()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating api key for project {projectId}", dto.ProjectId);
            return Result.Error($"Error creating api key for project {dto.ProjectId}");
        }
    }

    public async Task<Result> UpdateApiKeyAsync(ApiKeyDto dto)
    {
        try
        {
            var key = await dbHelper.ExecuteAsync(async dbContext => 
                await dbContext.ApiKeys.FindAsync(dto.Id));
            
            if (key is null)
                return Result.NotFound();
            
            dto.Adapt(key, 
                TypeAdapterConfig<ApiKeyDto, ApiKey>
                .NewConfig()
                .Ignore(k => k.ProjectId)
                .Config);
            
            await dbHelper.ExecuteAsync(async dbContext => await dbContext.SaveChangesAsync());

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating api key for project {projectId}", dto.ProjectId);
            return Result.Error($"Error updating api key for project {dto.ProjectId}");
        }
    }

    public async Task<Result> DeleteApiKeyAsync(string apiKeyId)
    {
        try
        {
            var key = await dbHelper.ExecuteAsync(async dbContext => 
                await dbContext.ApiKeys.FindAsync(apiKeyId));
            
            if (key is null)
                return Result.NotFound();
            
            await dbHelper.ExecuteAsync(async dbContext =>
            {
                dbContext.ApiKeys.Remove(key);
                await dbContext.SaveChangesAsync();
            });

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting api key {apiKey}", apiKeyId);
            return Result.Error($"Error deleting api key  {apiKeyId}");
        }
    }
}