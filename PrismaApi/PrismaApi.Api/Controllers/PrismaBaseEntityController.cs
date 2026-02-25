using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Graph;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace PrismaApi.Api.Controllers;

[ApiController]
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

        await _dbContext.RebuildTablesAsync();
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
