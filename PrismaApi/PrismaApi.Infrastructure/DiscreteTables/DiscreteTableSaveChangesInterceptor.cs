using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PrismaApi.Infrastructure.DiscreteTables;

public sealed class DiscreteTableSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly DiscreteTableEventHandler _eventHandler = new();
    private readonly DiscreteTableRecalculator _recalculator = new();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is AppDbContext dbContext && !dbContext.IsDiscreteTableEventDisabled)
        {
            _eventHandler.ProcessSessionChanges(dbContext);
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext dbContext && !dbContext.IsDiscreteTableEventDisabled)
        {
            _eventHandler.ProcessSessionChanges(dbContext);
        }

        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is AppDbContext dbContext && !dbContext.IsDiscreteTableEventDisabled)
        {
            _recalculator.RecalculateAsync(dbContext, dbContext.DiscreteTableSessionInfo, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext dbContext && !dbContext.IsDiscreteTableEventDisabled)
        {
            await _recalculator.RecalculateAsync(dbContext, dbContext.DiscreteTableSessionInfo, cancellationToken);
        }

        return result;
    }
}
