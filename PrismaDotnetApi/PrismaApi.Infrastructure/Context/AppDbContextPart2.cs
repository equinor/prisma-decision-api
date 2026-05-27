using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Caching;

namespace PrismaApi.Infrastructure.Context;

public partial class AppDbContext : DbContext
{
    private void InvalidateAssessmentsCache()
    {
        var assessmentEntries = ChangeTracker.Entries<Assessment>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
        var assessmentIds = assessmentEntries.Select(e => e.Entity.Id).ToList();

        var decisionQualityAssessmentEntries = ChangeTracker.Entries<DecisionQualityAssessment>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        HashSet<Guid> affectedProjectIds = assessmentEntries
            .Select(e => e.Entity.ProjectId)
            .ToHashSet();
        affectedProjectIds.UnionWith(decisionQualityAssessmentEntries.Select(e => e.Entity.ProjectId));
        foreach (var projectId in affectedProjectIds)
        {
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetAssessmentKey(projectId) });
        }
    }

    public async Task InvalidateCacheAsync()
    {
        InvalidateAssessmentsCache();
        var userEntries = ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            
        var projectRolesEntries = ChangeTracker.Entries<ProjectRole>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            
        var edgeEntries = ChangeTracker.Entries<Edge>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            
        var issueEntries = ChangeTracker.Entries<Issue>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        var nodeEntries = ChangeTracker.Entries<Node>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            
        var uncertaintyEntries = ChangeTracker.Entries<Uncertainty>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            
        var decisionEntries = ChangeTracker.Entries<Decision>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            
        var optionEntries = ChangeTracker.Entries<Option>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            
        var outcomeEntries = ChangeTracker.Entries<Outcome>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
            
        var discreteProbEntries = ChangeTracker.Entries<DiscreteProbability>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        var discreteUtilityEntries = ChangeTracker.Entries<DiscreteUtility>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
        
        HashSet<Guid> affectedProjectIds = [];
        HashSet<string> affectedUserCacheKeys = [];
        foreach (var entry in projectRolesEntries)
        {
            // if the user role is being modified or deleted, 
            // we need to invalidate the cache for that user and the project
            if (AppOptions.IsPublicInstance)
            {
                string userName;
                if (entry.Entity.User == null)
                {
                    userName = await Users
                        .Where(u => u.Id == entry.Entity.UserId)
                        .Select(e => e.Name)
                        .FirstAsync();
                }
                else
                {
                    userName = entry.Entity.User.Name;
                }
                affectedUserCacheKeys.Add(CacheKeys.GetUserKey(userName));
            }
            else
            {
                affectedUserCacheKeys.Add(entry.Entity.UserId);
            }
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in userEntries)
        {
            affectedUserCacheKeys.Add(entry.Entity.Id);
        }

        foreach (var entry in edgeEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in issueEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in nodeEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in uncertaintyEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in decisionEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in optionEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in outcomeEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in discreteProbEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var entry in discreteUtilityEntries)
        {
            affectedProjectIds.Add(entry.Entity.ProjectId);
        }

        foreach (var userId in affectedUserCacheKeys)
        {
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = userId });
        }
        foreach (var projectId in affectedProjectIds)
        {
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetInfluenceDiagramKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetIssuesInProjectKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetEdgesInProjectKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetNodesInProjectKey(projectId) });
        }
    }
}
