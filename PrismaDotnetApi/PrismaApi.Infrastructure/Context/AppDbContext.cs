using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Interfaces;
using PrismaApi.Infrastructure.DiscreteTables;

namespace PrismaApi.Infrastructure.Context;

public partial class AppDbContext : DbContext
{
    private readonly IMemoryCache _cache;
    public readonly AppDbContextOptions AppOptions;

    public AppDbContext(DbContextOptions<AppDbContext> options, IMemoryCache cache, AppDbContextOptions appOptions) : base(options)
    {
        _cache = cache;
        AppOptions = appOptions;
    }

    public DiscreteTableSessionInfo DiscreteTableSessionInfo { get; } = new();
    public bool IsDiscreteTableEventDisabled { get; set; }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<NodeStyle> NodeStyles => Set<NodeStyle>();
    public DbSet<Edge> Edges => Set<Edge>();
    public DbSet<Decision> Decisions => Set<Decision>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Outcome> Outcomes => Set<Outcome>();
    public DbSet<Uncertainty> Uncertainties => Set<Uncertainty>();
    public DbSet<Utility> Utilities => Set<Utility>();
    public DbSet<ValueMetric> ValueMetrics => Set<ValueMetric>();
    public DbSet<DiscreteProbability> DiscreteProbabilities => Set<DiscreteProbability>();
    public DbSet<DiscreteProbabilityParentOutcome> DiscreteProbabilityParentOutcomes => Set<DiscreteProbabilityParentOutcome>();
    public DbSet<DiscreteProbabilityParentOption> DiscreteProbabilityParentOptions => Set<DiscreteProbabilityParentOption>();
    public DbSet<DiscreteUtility> DiscreteUtilities => Set<DiscreteUtility>();
    public DbSet<DiscreteUtilityParentOutcome> DiscreteUtilityParentOutcomes => Set<DiscreteUtilityParentOutcome>();
    public DbSet<DiscreteUtilityParentOption> DiscreteUtilityParentOptions => Set<DiscreteUtilityParentOption>();
    public DbSet<Strategy> Strategies => Set<Strategy>();
    public DbSet<StrategyOption> StrategyOptions => Set<StrategyOption>();
    public DbSet<Objective> Objectives => Set<Objective>();
    public DbSet<ProjectRole> ProjectRoles => Set<ProjectRole>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<DecisionQualityAssessment> DecisionQualityAssessments => Set<DecisionQualityAssessment>();
    public DbSet<BoardNode> BoardNodes => Set<BoardNode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Project.OnModelConfiguring(modelBuilder);
        Issue.OnModelConfiguring(modelBuilder);
        Node.OnModelConfiguring(modelBuilder);
        NodeStyle.OnModelConfiguring(modelBuilder);
        Edge.OnModelConfiguring(modelBuilder);
        Decision.OnModelConfiguring(modelBuilder);
        Option.OnModelConfiguring(modelBuilder);
        Outcome.OnModelConfiguring(modelBuilder);
        Uncertainty.OnModelConfiguring(modelBuilder);
        Utility.OnModelConfiguring(modelBuilder);
        DiscreteProbability.OnModelConfiguring(modelBuilder);
        DiscreteProbabilityParentOutcome.OnModelConfiguring(modelBuilder);
        DiscreteProbabilityParentOption.OnModelConfiguring(modelBuilder);
        DiscreteUtility.OnModelConfiguring(modelBuilder);
        DiscreteUtilityParentOutcome.OnModelConfiguring(modelBuilder);
        DiscreteUtilityParentOption.OnModelConfiguring(modelBuilder);
        ValueMetric.OnModelConfiguring(modelBuilder);
        Strategy.OnModelConfiguring(modelBuilder);
        StrategyOption.OnModelConfiguring(modelBuilder);
        Objective.OnModelConfiguring(modelBuilder);
        ProjectRole.OnModelConfiguring(modelBuilder);
        User.OnModelConfiguring(modelBuilder);
        Assessment.OnModelConfiguring(modelBuilder);
        DecisionQualityAssessment.OnModelConfiguring(modelBuilder);
        BoardNode.OnModelConfiguring(modelBuilder);
    }

    private IEnumerable<EntityEntry<T>> GetChangedEntries<T>() where T : class =>
        ChangeTracker.Entries<T>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

    public override int SaveChanges()
    {
        throw new InvalidOperationException("Use SaveChangesAsync instead.");
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        await EnforceMinimumProjectRoles(cancellationToken);
        await OnNodeDeletedCleanupAsync(cancellationToken);
        await OnOutcomeDeletedCleanupAsync(cancellationToken);
        await OnOptionDeletedCleanupAsync(cancellationToken);
        // invalidate before savechanges because save changes clears out the change tracker
        await InvalidateCacheAsync();
        return await base.SaveChangesAsync(cancellationToken);
        
    }


    public async Task<int> SaveChangesWhileDuplicatingAsync(CancellationToken cancellationToken = default)
    {
        // Alternitive design can be found at https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/events

        // no need to invalidate cache here since the duplicated entities will have new ids 
        // and won't affect existing cache entries
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                var preserveCreatedTimestamp = entry.Entity is Outcome or Option;
                if (!preserveCreatedTimestamp || entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                }
            }

            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    private async Task EnforceMinimumProjectRoles(CancellationToken cancellationToken)
    {
        // get the roles that have been deleted or modified and group by the project id
        // to make it easier to iterate over the projects
        var affectedByProject = ChangeTracker
            .Entries<ProjectRole>()
            .Where(e => e.State == EntityState.Deleted || e.State == EntityState.Modified)
            .GroupBy(e => e.Entity.ProjectId)
            .ToDictionary(g => g.Key, g => g);

        if (affectedByProject.Count == 0)
            return;

        foreach (var (projectId, roles) in affectedByProject)
        {
            var facilitatorsBeingRemoved = roles.Count(role =>
                role.State == EntityState.Deleted
                    ? string.Equals(role.Entity.Role, ProjectRoleType.Facilitator.ToString(), StringComparison.OrdinalIgnoreCase)
                    : string.Equals(
                        role.OriginalValues.GetValue<string>(nameof(ProjectRole.Role)),
                        ProjectRoleType.Facilitator.ToString(),
                        StringComparison.OrdinalIgnoreCase));

            if (facilitatorsBeingRemoved == 0)
                continue;

            var currentFacilitatorCount = await ProjectRoles
                .AsNoTracking()
                .CountAsync(r => r.ProjectId == projectId &&
                                 r.Role.ToUpper() == ProjectRoleType.Facilitator.ToString().ToUpper(),
                             cancellationToken);

            if (currentFacilitatorCount - facilitatorsBeingRemoved <= 0)
                throw new InvalidOperationException("Projects must have at least one Facilitator.");
        }
    }

    private async Task OnNodeDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedNodeIds = GetDeletedEntityIds<Node>();

        if (deletedNodeIds.Any())
        {
            // Delete tail edges that reference the deleted nodes
            var tailEdgesToDelete = await Edges
                .Where(e => deletedNodeIds.Contains(e.TailId) || deletedNodeIds.Contains(e.HeadId))
                .ToListAsync(cancellationToken);

            Edges.RemoveRange(tailEdgesToDelete);
        }
    }

    private async Task OnOutcomeDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedOutcomeIds = GetDeletedEntityIds<Outcome>();

        if (deletedOutcomeIds.Any())
        {
            // Load the parent outcome relationships and the discrete probabilities they reference
            var affectedProbParentOutcomes = await DiscreteProbabilityParentOutcomes
                .AsSplitQuery()
                .Where(po => deletedOutcomeIds.Contains(po.ParentOutcomeId))
                .Include(po => po.DiscreteProbability)
                    .ThenInclude(dp => dp!.ParentOptions)
                .Include(po => po.DiscreteProbability)
                    .ThenInclude(dp => dp!.ParentOutcomes)
                .ToListAsync(cancellationToken);

            var affectedProbs = affectedProbParentOutcomes
                .Select(po => po.DiscreteProbability)
                .Where(dp => dp != null)
                .Distinct()
                .Cast<DiscreteProbability>()
                .ToList();

            DiscreteProbabilities.RemoveRange(affectedProbs);

            // Load the parent outcome relationships and the discrete utilities they reference
            var affectedUtilParentOutcomes = await DiscreteUtilityParentOutcomes
                .AsSplitQuery()
                .Where(uo => deletedOutcomeIds.Contains(uo.ParentOutcomeId))
                .Include(uo => uo.DiscreteUtility)
                    .ThenInclude(du => du!.ParentOptions)
                .Include(uo => uo.DiscreteUtility)
                    .ThenInclude(du => du!.ParentOutcomes)
                .ToListAsync(cancellationToken);

            var affectedUtils = affectedUtilParentOutcomes
                .Select(uo => uo.DiscreteUtility)
                .Where(du => du != null)
                .Distinct()
                .Cast<DiscreteUtility>()
                .ToList();

            DiscreteUtilities.RemoveRange(affectedUtils);
        }
    }
    private async Task OnOptionDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedOptionIds = GetDeletedEntityIds<Option>();

        if (deletedOptionIds.Any())
        {

            var strategyOptionsToDelete = await StrategyOptions
                .Where(e => deletedOptionIds.Contains(e.OptionId))
                .ToListAsync();
            StrategyOptions.RemoveRange(strategyOptionsToDelete);

            // Load the parent option relationships and the discrete probabilities they reference
            var affectedProbParentOptions = await DiscreteProbabilityParentOptions
                .AsSplitQuery()
                .Where(po => deletedOptionIds.Contains(po.ParentOptionId))
                .Include(po => po.DiscreteProbability)
                    .ThenInclude(dp => dp!.ParentOptions)
                .Include(po => po.DiscreteProbability)
                    .ThenInclude(dp => dp!.ParentOutcomes)
                .ToListAsync(cancellationToken);

            var affectedProbs = affectedProbParentOptions
                .Select(po => po.DiscreteProbability)
                .Where(dp => dp != null)
                .Distinct()
                .Cast<DiscreteProbability>()
                .ToList();

            DiscreteProbabilities.RemoveRange(affectedProbs);
            DiscreteProbabilityParentOptions.RemoveRange(affectedProbParentOptions);

            // Load the parent option relationships and the discrete utilities they reference
            var affectedUtilParentOptions = await DiscreteUtilityParentOptions
                .AsSplitQuery()
                .Where(uo => deletedOptionIds.Contains(uo.ParentOptionId))
                .Include(uo => uo.DiscreteUtility)
                    .ThenInclude(du => du!.ParentOptions)
                .Include(uo => uo.DiscreteUtility)
                    .ThenInclude(du => du!.ParentOutcomes)
                .ToListAsync(cancellationToken);

            var affectedUtils = affectedUtilParentOptions
                .Select(uo => uo.DiscreteUtility)
                .Where(du => du != null)
                .Distinct()
                .Cast<DiscreteUtility>()
                .ToList();

            DiscreteUtilities.RemoveRange(affectedUtils);
            DiscreteUtilityParentOptions.RemoveRange(affectedUtilParentOptions);
        }
    }

    private HashSet<Guid> GetDeletedEntityIds<TEntity>()
        where TEntity : class, IBaseEntity<Guid>
    {
        return ChangeTracker.Entries<TEntity>()
            .Where(e => e.State == EntityState.Deleted)
            .Select(e => e.Entity.Id)
            .ToHashSet();
    }


    public EntityEntry<IBaseEntity<Guid>> CreateEntryFromCollectionAsAdded(IBaseEntity<Guid> entity)
    {
        Entry(entity).State = EntityState.Added;
        return base.Add(entity);
    }


}
