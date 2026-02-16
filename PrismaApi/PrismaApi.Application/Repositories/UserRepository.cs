using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class UserRepository : BaseRepository<User, Guid>
{
    public UserRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<User> Query()
    {
        return DbContext.Users
            .Include(u => u.ProjectRoles);
    }

    public Task<User?> GetByAzureIdAsync(string azureId)
    {
        return DbContext.Users
            .Include(u => u.ProjectRoles)
            .FirstOrDefaultAsync(u => u.AzureId == azureId);
    }
}
