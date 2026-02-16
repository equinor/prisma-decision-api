using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class UncertaintyRepository : BaseRepository<Uncertainty, Guid>
{
    public UncertaintyRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    protected override IQueryable<Uncertainty> Query()
    {
        return DbContext.Uncertainties
            .Include(u => u.Outcomes);
    }
}
