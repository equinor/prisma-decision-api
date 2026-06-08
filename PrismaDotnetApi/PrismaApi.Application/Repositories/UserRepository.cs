using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.IdentityGovernance.EntitlementManagement.Assignments.AdditionalAccessWithAccessPackageIdWithIncompatibleAccessPackageId;
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

        var auditableEntityTypes = DbContext.Model.GetEntityTypes()
            .Where(t => typeof(AuditableEntity).IsAssignableFrom(t.ClrType) && !t.ClrType.IsAbstract);

        foreach (var entityType in auditableEntityTypes)
        {
            var tableName = entityType.GetTableName();
            try
            {
                
                await DbContext.Database.ExecuteSqlRawAsync($"""
                    UPDATE [{tableName}]
                    SET CreatedById = '{DomainConstants.DeletedUserId}'
                    WHERE CreatedById IN ({inClause});
                    UPDATE [{tableName}]
                    SET UpdatedById = '{DomainConstants.DeletedUserId}'
                    WHERE UpdatedById IN ({inClause});
                    """, [.. idParams.Concat(idParams).Cast<object>()], ct);
            }
            catch (Exception e)
            {
                
                throw e;
            }
        }
        // var auditableEntitiesToUpdate = DbContext.

        // foreach (var auditableEntity in auditableEntitiesToUpdate)
        // {
        //     if (ids.Contains(auditableEntity.UpdatedById))
        //     {
        //         auditableEntity.UpdatedById = DomainConstants.DeletedUserId;
        //     }

        //     if (ids.Contains(auditableEntity.CreatedById))
        //     {
        //         auditableEntity.CreatedById = DomainConstants.DeletedUserId;
        //     }
        // }

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
