namespace CMS.Main.Data;

public class DbContextConcurrencyHelper(ApplicationDbContext dbContext)
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public async Task ExecuteAsync(Func<ApplicationDbContext, Task> action)
    {
        await semaphore.WaitAsync();
        try
        {
            await action(dbContext);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<TResult> ExecuteAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> func)
    {
        await semaphore.WaitAsync();
        try
        {
            return await func(dbContext);
        }
        finally
        {
            semaphore.Release();
        }
    }
}