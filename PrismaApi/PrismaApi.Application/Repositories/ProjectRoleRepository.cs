using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;

namespace PrismaApi.Application.Repositories;

public class ProjectRoleRepository : BaseRepository<ProjectRole, Guid>, IProjectRoleRepository
{
    public ProjectRoleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task UpdateRangeAsync(IEnumerable<ProjectRole> incomingEntities, Expression<Func<ProjectRole, bool>> filterPredicate, CancellationToken ct = default)
    {
        var incomingList = incomingEntities.ToList();
        if (incomingList.Count == 0)
        {
            return;
        }

        var entities = await GetByIdsAsync(incomingList.Select(e => e.Id), filterPredicate: filterPredicate, ct: ct);
        // filter out entities not found
        if (entities.Count != incomingList.Count)
            incomingList = incomingList.Where(e => entities.Select(x => x.Id).Contains(e.Id)).ToList();
        entities.Update(incomingList, DbContext);

        await DbContext.SaveChangesAsync(ct);
    }

    public async Task<bool> IsUserFacilitatorFromProjectIdsAsync(List<Guid> projectIds, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        return projectIds.Count == await DbContext.ProjectRoles
            .Where(x => projectIds.Contains(x.ProjectId) 
                && x.Role.ToUpper() == ProjectRoleType.Facilitator.ToString().ToUpper()
                && x.UserId == userDto.Id)
            .DistinctBy(y => y.ProjectId) // in case user has several facilitator roles for one project
            .CountAsync(ct);
    }

    public async Task<bool> IsUserFacilitatorFromRoleIdsAsync(List<Guid> roleIds, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        return roleIds.Count == await DbContext.ProjectRoles
            .Where(x => roleIds.Contains(x.Id)
                && x.Role.ToUpper() == ProjectRoleType.Facilitator.ToString().ToUpper()
                && x.UserId == userDto.Id)
            .DistinctBy(y => y.ProjectId) // incase user has several facilitator roles for one project
            .CountAsync(ct);
    }

    protected override IQueryable<ProjectRole> Query()
    {
        return DbContext.ProjectRoles
            .Include(pr => pr.User)
            .Include(pr => pr.Project);
    }
}
