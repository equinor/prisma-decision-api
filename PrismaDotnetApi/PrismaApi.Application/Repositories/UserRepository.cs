using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace PrismaApi.Application.Repositories;

public class UserRepository : BaseRepository<User, string>, IUserRepository
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task UpdateRangeAsync(IEnumerable<User> incomingEntities, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), ct: ct);
        foreach (var entity in entities)
        {
            var incomingEntity = incomingList.FirstOrDefault(x => x.Id == entity.Id);
            if (incomingEntity == null)
            {
                continue;
            }

            entity.Name = incomingEntity.Name;
        }

        await DbContext.SaveChangesAsync(ct);
    }

    protected override IQueryable<User> Query()
    {
        return DbContext.Users
            .Include(u => u.ProjectRoles);
    }

    private async Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
    {
        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Name.ToLower() == userName.ToLower(), ct);
    }

    public async Task<User> GetOrAddByIdAsync(UserIncomingDto dto, CancellationToken ct = default)
    {
        var existingUser = await GetByIdAsync(dto.Id, ct: ct);
        if (existingUser != null)
        {
            return existingUser;
        }

        User user = dto.ToEntity();
        await DbContext.Users.AddAsync(user, ct);
        await DbContext.SaveChangesAsync(ct);

        return user;
    }
    public async Task<User> GetOrAddByUserNameAsync(UserIncomingDto dto, CancellationToken ct = default)
    {
        var existingUser = await GetByUserNameAsync(dto.Name, ct: ct);
        if (existingUser != null)
        {
            return existingUser;
        }

        User user = dto.ToEntity();
        await DbContext.Users.AddAsync(user, ct);
        await DbContext.SaveChangesAsync(ct);

        return user;
    }

    /// <summary>
    /// Deletes users by their ids. 
    /// Before deleting, it updates all auditable entities that reference the user to point to the deleted user entry. 
    /// This is done to avoid foreign key constraint violations when deleting a user.
    /// Also, deletes all related project roles for the user
    /// </summary>
    public override async Task DeleteByIdsAsync(IEnumerable<string> ids, Expression<Func<User, bool>>? filterPredicate = null, CancellationToken ct = default)
    {
        var idsList = ids.ToList();
        var entries = await DbContext.Users
            .OptionalWhere(filterPredicate)
            .Where(u => idsList.Contains(u.Id))
            .ToListAsync(cancellationToken: ct);

        if (entries.Count == 0)
            return;

        var auditableEntityTypes = DbContext.Model.GetEntityTypes()
            .Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType) && !t.ClrType.IsAbstract)
            .Select(t => t.ClrType)
            .Distinct()
            .ToList();

        foreach (var auditableEntityType in auditableEntityTypes)
        {
            await UpdateAuditableReferencesByTypeAsync(auditableEntityType, idsList, DomainConstants.DeletedUserId, ct);
        }

        // delete project roles
        var projectRoles = await DbContext.ProjectRoles
            .Where(e => idsList.Contains(e.UserId))
            .ToListAsync(cancellationToken: ct);
        DbContext.ProjectRoles.RemoveRange(projectRoles);

        foreach (var entry in entries)
        {
            DbContext.Users.Remove(entry);
        }

        await DbContext.SaveChangesAsync(ct);
    }

    private async Task UpdateAuditableReferencesByTypeAsync(
        Type auditableEntityType,
        IReadOnlyCollection<string> idsList,
        string userId,
        CancellationToken ct)
    {
        // Use reflection to call the generic method with the specific entity type
        var UpdateAuditableReferencesByTypeMethod = typeof(UserRepository)
            .GetMethod(nameof(UpdateAuditableReferencesByTypeAsync), BindingFlags.NonPublic | BindingFlags.Static)!;
        var genericMethod = UpdateAuditableReferencesByTypeMethod.MakeGenericMethod(auditableEntityType);
        var updateTask = (Task<int>?)genericMethod.Invoke(null, [DbContext, idsList, userId, ct]);
        if (updateTask == null)
        {
            throw new InvalidOperationException($"Could not execute auditable reference update for type '{auditableEntityType.Name}'.");
        }
        await updateTask;
    }

    private static Task<int> UpdateAuditableReferencesByTypeAsync<TEntity>(
        AppDbContext dbContext,
        List<string> idsList,
        string userId,
        CancellationToken ct) where TEntity : AuditableEntity
    {
        // Use ExecuteUpdateAsync to update the CreatedById and UpdatedById properties to userId for all entities of type TEntity 
        return dbContext
            .Set<TEntity>()
            .Where(e => idsList.Contains(e.CreatedById) || idsList.Contains(e.UpdatedById))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        e => e.CreatedById,
                        e => idsList.Contains(e.CreatedById) ? userId : e.CreatedById)
                    .SetProperty(
                        e => e.UpdatedById,
                        e => idsList.Contains(e.UpdatedById) ? userId : e.UpdatedById),
                cancellationToken: ct);
    }
}
