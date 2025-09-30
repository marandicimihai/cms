using CMS.Main.DTOs.ApiKey;
using System;

namespace CMS.Main.Services.State;

public class ApiKeyStateService
{
    public event Action<ApiKeyDto>? ApiKeyCreated;

    public void NotifyCreated(ApiKeyDto apiKey)
    {
        ApiKeyCreated?.Invoke(apiKey);
    }
}

