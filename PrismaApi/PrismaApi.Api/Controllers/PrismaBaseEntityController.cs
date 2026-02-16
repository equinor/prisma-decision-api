using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers;

public abstract class PrismaBaseEntityController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    protected PrismaBaseEntityController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            return Task.CompletedTask;
        }

        return StartTransactionAsync(cancellationToken);
    }

    protected async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    protected async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    private async Task StartTransactionAsync(CancellationToken cancellationToken)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}
