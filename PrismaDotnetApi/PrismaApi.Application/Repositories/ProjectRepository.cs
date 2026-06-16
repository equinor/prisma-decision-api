using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Extensions;
using PrismaApi.Infrastructure.Context;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class ProjectRepository : BaseRepository<Project, Guid>, IProjectRepository
{
    public readonly IProjectRoleRepository _repo;
    public ProjectRepository(AppDbContext dbContext, IProjectRoleRepository repo) : base(dbContext)
    {
        _repo = repo;
    }

    public override async Task DeleteByIdsAsync(IEnumerable<Guid> ids, Expression<Func<Project, bool>>? filterPredicate = null, CancellationToken ct = default)
    {
        var edgesToDelete = await DbContext.Edges
            .Where(x => ids.Contains(x.HeadNode!.ProjectId) || ids.Contains(x.TailNode!.ProjectId))
            .ToListAsync(ct);
        DbContext.Edges.RemoveRange(edgesToDelete);
        await base.DeleteByIdsAsync(ids, filterPredicate, ct);
    }

    public async Task<ICollection<Project>> GetProjectsWhereUserHasAccess(ICollection<Guid> projectIds, string userId, CancellationToken ct = default)
    {
        return await GetByIdsAsync(
            projectIds,
            withTracking: false,
            filterPredicate: p => p.ProjectRoles.Any(r => r.UserId == userId),
            ct: ct
        );
    }


    public async Task<IEnumerable<Project>> UpdateRangeAsync(IEnumerable<Project> incomingEntities, UserOutgoingDto userDto, Expression<Func<Project, bool>> filterPredicate, CancellationToken ct = default)
    {
        var entities = await GetByIdsAsync(incomingEntities.Select(e => e.Id), withTracking: true, filterPredicate: filterPredicate, ct: ct);
        foreach (var entity in entities)
        {
            var incomingEntity = incomingEntities.First(x => x.Id == entity.Id);

            // if user is not a facillitator, they cannnot change the role type.
            bool isUserFacillitator = entity.ProjectRoles
                .Any(r => r.Role.IsFacilitator() && r.UserId == userDto.Id);

            if (!isUserFacillitator)
            {
                throw new UnauthorizedAccessException("Only facilitators can update project information.");
            }

            entity.Name = incomingEntity.Name;
            entity.OpportunityStatement = incomingEntity.OpportunityStatement;
            entity.Public = incomingEntity.Public;
            entity.ParentProjectId = incomingEntity.ParentProjectId;
            entity.ParentProjectName = incomingEntity.ParentProjectName;
            entity.EndDate = incomingEntity.EndDate;
            entity.UpdatedById = incomingEntity.UpdatedById;
            
            if (incomingEntity.ProjectRoles.Count == 0)
            {
                throw new InvalidOperationException("At least one project role is required.");
            }
            if (!incomingEntity.ProjectRoles.Any(x => x.Role.IsFacilitator()))
            {
                throw new InvalidOperationException(ExceptionMessages.MinimumFacilitatorRequirement);
            }
            entity.ProjectRoles.Update(incomingEntity.ProjectRoles, DbContext);
            entity.Objectives.Update(incomingEntity.Objectives, DbContext);
            entity.Strategies.Update(incomingEntity.Strategies, DbContext);
            entity.BoardNodes.Update(incomingEntity.BoardNodes, DbContext);
        }

        await DbContext.SaveChangesAsync(ct);
        return incomingEntities;
    }

    protected override IQueryable<Project> Query()
    {
        return DbContext.Projects
            .Include(p => p.Objectives)
            .Include(p => p.BoardNodes)
            .Include(p => p.ProjectRoles)
                .ThenInclude(pr => pr.User)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.StrategyOptions)
                .ThenInclude(so => so.Option);
    }


}
