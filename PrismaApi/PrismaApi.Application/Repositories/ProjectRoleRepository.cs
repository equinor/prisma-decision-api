using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class ProjectRoleRepository : BaseRepository<ProjectRole, System.Guid>
{
    public ProjectRoleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<ProjectRole> Query()
    {
        return DbContext.ProjectRoles
            .Include(pr => pr.User)
            .Include(pr => pr.Project);
    }
}
