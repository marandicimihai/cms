using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMS.Main.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CMS.Tests;

public class DbContextConcurrencyHelperTests
{
    private static ApplicationDbContext CreateInMemoryContext(string name)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ExecuteAsync_ActionRuns()
    {
        using var ctx = CreateInMemoryContext(nameof(ExecuteAsync_ActionRuns));
        var helper = new DbContextConcurrencyHelper(ctx);
        var flag = false;

        await helper.ExecuteAsync(async db =>
        {
            flag = true;
            await Task.Yield();
        });

        Assert.True(flag);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsValue()
    {
        using var ctx = CreateInMemoryContext(nameof(ExecuteAsync_ReturnsValue));
        var helper = new DbContextConcurrencyHelper(ctx);

        var result = await helper.ExecuteAsync(async db =>
        {
            await Task.Delay(10);
            return 42;
        });

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_SerializesConcurrentCalls()
    {
        using var ctx = CreateInMemoryContext(nameof(ExecuteAsync_SerializesConcurrentCalls));
        var helper = new DbContextConcurrencyHelper(ctx);

        var concurrentInside = 0;
        var maxObserved = 0;

        async Task Work()
        {
            await helper.ExecuteAsync(async db =>
            {
                var now = Interlocked.Increment(ref concurrentInside);
                maxObserved = Math.Max(maxObserved, now);
                // simulate work
                await Task.Delay(50);
                Interlocked.Decrement(ref concurrentInside);
            });
        }

        var tasks = Enumerable.Range(0, 5).Select(_ => Work()).ToArray();
        await Task.WhenAll(tasks);

        Assert.Equal(1, maxObserved); // semaphore should ensure only 1 inside at a time
    }

    [Fact]
    public async Task ExecuteAsync_ExceptionBubblesAndReleasesSemaphore()
    {
        using var ctx = CreateInMemoryContext(nameof(ExecuteAsync_ExceptionBubblesAndReleasesSemaphore));
        var helper = new DbContextConcurrencyHelper(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await helper.ExecuteAsync(_ => throw new InvalidOperationException("Boom")));

        var ran = false;
        await helper.ExecuteAsync(async _ =>
        {
            ran = true;
            await Task.Yield();
        });
        Assert.True(ran);
    }

    [Fact]
    public async Task ExecuteAsync_ResultOverlappingCalls_AllTasksRun()
    {
        using var ctx = CreateInMemoryContext(nameof(ExecuteAsync_ResultOverlappingCalls_AllTasksRun));
        var helper = new DbContextConcurrencyHelper(ctx);

        var order = new ConcurrentQueue<int>();

        Task<int> Enqueue(int id)
        {
            return helper.ExecuteAsync(async _ =>
            {
                order.Enqueue(id);
                await Task.Delay(10);
                return id;
            });
        }

        var tasks = new[] { Enqueue(1), Enqueue(2), Enqueue(3) };
        var results = await Task.WhenAll(tasks);

        Assert.Equal(3, order.Count);
        Assert.True(new[] { 1, 2, 3 }.All(id => order.Contains(id)));
        Assert.Equal(3, results.Distinct().Count());
    }
}