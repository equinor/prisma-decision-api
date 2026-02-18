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
    private readonly GraphServiceClient _graphServiceClient;
    private readonly UserRepository _userRepository;

    protected PrismaBaseEntityController(
        AppDbContext dbContext,
        GraphServiceClient graphServiceClient,
        UserRepository userRepository

    )
    {
        _dbContext = dbContext;
        _graphServiceClient = graphServiceClient;
        _userRepository = userRepository;
    }

    protected async Task<UserOutgoingDto> GetOrCreateUserFromGraphMeAsync()
    {
        var graphUser = await _graphServiceClient.Me.GetAsync();
        if (graphUser == null || graphUser.Id == null) throw new Exception("User not found");
        var userDto = new UserIncomingDto
        {
            AzureId = graphUser.Id,
            Name = graphUser.DisplayName ?? "",
        };

        return (await _userRepository.GetOrAddByAzureIdAsync(userDto)).ToOutgoingDto();

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
