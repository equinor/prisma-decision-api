using Microsoft.EntityFrameworkCore;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Services;

public class StrategyTableService
{
    private readonly AppDbContext _dbContext;

    public StrategyTableService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RemoveOutStrategyOptionsFromOptionIdsAsync()
    {
        var ids = _dbContext.DiscreteTableSessionInfo.OptionsToRemoveFromStrategies;
        var entities = await _dbContext.StrategyOptions
            .Where(e => ids.Contains(e.OptionId))
            .ToListAsync();
        _dbContext.StrategyOptions.RemoveRange(entities);
        await _dbContext.SaveChangesAsync();
    }
}
