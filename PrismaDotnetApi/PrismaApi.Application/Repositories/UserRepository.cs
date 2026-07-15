using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Extensions;
using System.Linq.Expressions;

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
        var entries = await DbContext.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken: ct);

        if (entries.Count == 0)
            return;

        // update all auditable entities to point to deleted user entry
        var connection = DbContext.Database.GetDbConnection();
        using var cmd = connection.CreateCommand();
        var idParams = ids.Select((id, i) =>
        {
            var param = cmd.CreateParameter();
            param.ParameterName = $"@id{i}";
            param.Value = id;
            return param;
        }).ToArray();
        var inClause = string.Join(",", idParams.Select(p => p.ParameterName));

        // get all auditable entity types that need the user id updated to the deleted user id. 
        // This is done to avoid foreign key constraint violations when deleting a user.
        var auditableEntityTypes = DbContext.Model.GetEntityTypes()
            .Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType) && !t.ClrType.IsAbstract);

        foreach (var entityType in auditableEntityTypes)
        {
            var tableName = entityType.GetTableName();
                
            // using raw SQL over the parameterized ExecuteSqlAsync due to issues with the in clause.
            // ExecuteSqlAsync protects against SQL injection, 
            // but all inputed data are controlled by the api and takes no user input.
            await DbContext.Database.ExecuteSqlRawAsync($"""
                UPDATE [{tableName}]
                SET CreatedById = '{DomainConstants.DeletedUserId}'
                WHERE CreatedById IN ({inClause});
                UPDATE [{tableName}]
                SET UpdatedById = '{DomainConstants.DeletedUserId}'
                WHERE UpdatedById IN ({inClause});
                """, [.. idParams.Cast<object>()], ct);
        }

        // delete project roles
        var projectRoles = await DbContext.ProjectRoles
            .Where(e => ids.Contains(e.UserId))
            .ToListAsync(cancellationToken: ct);

        DbContext.ProjectRoles.RemoveRange(projectRoles);
        foreach (var entry in entries)
        {
            DbContext.Users.Remove(entry);
        }
        await DbContext.SaveChangesAsync(ct);
    }
}
