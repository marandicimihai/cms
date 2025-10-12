using System.Security.Cryptography;
using Ardalis.Result;
using CMS.Main.Abstractions;
using CMS.Main.Data;
using Microsoft.EntityFrameworkCore;
using CMS.Main.Models;
using CMS.Main.Models.MappingConfig;
using Mapster;
using CMS.Main.DTOs;

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
            var project = await dbHelper.ExecuteAsync(async dbContext => 
                await dbContext.Projects.FindAsync(dto.ProjectId));

            if (project is null)
                return Result.NotFound();
            
            var newKey = dto.Adapt<ApiKey>();
            var keyValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var hashedKey = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(keyValue)));
            newKey.HashedKey = hashedKey;
            newKey.CreatedAt = DateTime.UtcNow;

            await dbHelper.ExecuteAsync(async dbContext =>
            {
                dbContext.ApiKeys.Add(newKey);
                await dbContext.SaveChangesAsync();
            });

            return Result.Success((keyValue, newKey.Adapt<ApiKeyDto>()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating api key '{apiKeyName}' for project {projectId}", dto.Name, dto.ProjectId);
            return Result.Error($"Error creating api key '{dto.Name}'");
        }
    }

    public async Task<Result> UpdateApiKeyAsync(ApiKeyDto dto)
    {
        try
        {
            var key = await dbHelper.ExecuteAsync(async dbContext => 
                await dbContext.ApiKeys.FirstOrDefaultAsync(k => k.Id == dto.Id));
            
            if (key is null)
                return Result.NotFound();

            dto.Adapt(key, MapsterConfig.EditApiKeyConfig);

            await dbHelper.ExecuteAsync(async dbContext => await dbContext.SaveChangesAsync());

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating api key '{apiKeyName}' for project {projectId}", dto.Name, dto.ProjectId);
            return Result.Error($"Error updating api key '{dto.Name}'");
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
            return Result.Error($"Error deleting api key {apiKeyId}");
        }
    }
}