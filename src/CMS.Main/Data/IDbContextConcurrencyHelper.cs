namespace CMS.Main.Data;

public interface IDbContextConcurrencyHelper
{
    Task ExecuteAsync(Func<ApplicationDbContext, Task> action);
    Task<TResult> ExecuteAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> func);
}

