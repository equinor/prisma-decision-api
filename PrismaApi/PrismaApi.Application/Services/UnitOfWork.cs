using Microsoft.EntityFrameworkCore.Storage;
using PrismaApi.Application.Interfaces;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            return;
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("A transaction has not been started.");

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
        catch (Exception)
        {
            if (_currentTransaction is not null)
                await _currentTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (_currentTransaction is not null)
            await _currentTransaction.RollbackAsync();
    }
}
