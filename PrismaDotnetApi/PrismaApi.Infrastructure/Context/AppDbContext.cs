using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Interfaces;
using PrismaApi.Infrastructure.Caching;
using PrismaApi.Infrastructure.DiscreteTables;

namespace PrismaApi.Infrastructure.Context;

public class AppDbContext : DbContext
{
    private readonly IMemoryCache _cache;
    public AppDbContext(DbContextOptions<AppDbContext> options, IMemoryCache cache) : base(options)
    {
        _cache = cache;
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

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.ParentProjectName).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.OpportunityStatement).HasMaxLength(DomainConstants.MaxLongStringLength);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.ProjectRoles)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Objectives)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Strategies)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Issues)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Nodes)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // will be cascade deleted through Issues

            entity.HasMany(e => e.BoardNodes)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Edges)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // will be cascade deleted through Issues -> Nodes
            entity.HasMany(e => e.Assessments)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Description).HasMaxLength(DomainConstants.MaxLongStringLength);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Boundary).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Node)
                .WithOne(e => e.Issue)
                .HasForeignKey<Node>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Decision)
                .WithOne(e => e.Issue)
                .HasForeignKey<Decision>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Uncertainty)
                .WithOne(e => e.Issue)
                .HasForeignKey<Uncertainty>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Utility)
                .WithOne(e => e.Issue)
                .HasForeignKey<Utility>(e => e.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasOne(e => e.NodeStyle)
                .WithOne(e => e.Node)
                .HasForeignKey<NodeStyle>(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.HeadEdges)
                .WithOne(e => e.HeadNode)
                .HasForeignKey(e => e.HeadId)
                .OnDelete(DeleteBehavior.NoAction); // deleted in cleanup

            entity.HasMany(e => e.TailEdges)
                .WithOne(e => e.TailNode)
                .HasForeignKey(e => e.TailId)
                .OnDelete(DeleteBehavior.NoAction); // deleted in cleanup
        });

        modelBuilder.Entity<NodeStyle>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Decision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasMany(e => e.Options)
                .WithOne(e => e.Decision)
                .HasForeignKey(e => e.DecisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<Outcome>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<Uncertainty>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Outcomes)
                .WithOne(e => e.Uncertainty)
                .HasForeignKey(e => e.UncertaintyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Utility>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<DiscreteProbability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OutcomeId);
            entity.HasIndex(e => e.UncertaintyId);
            entity.Property(e => e.Probability).HasPrecision(DomainConstants.FloatPrecision);

            entity.HasOne(e => e.Outcome)
                .WithMany()
                .HasForeignKey(e => e.OutcomeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Uncertainty)
                .WithMany(e => e.DiscreteProbabilities)
                .HasForeignKey(e => e.UncertaintyId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(e => e.ParentOutcomes)
                .WithOne(e => e.DiscreteProbability)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ParentOptions)
                .WithOne(e => e.DiscreteProbability)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscreteProbabilityParentOutcome>(entity =>
        {
            entity.HasKey(e => new { e.DiscreteProbabilityId, e.ParentOutcomeId });

            entity.HasOne(e => e.DiscreteProbability)
                .WithMany(d => d.ParentOutcomes)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DiscreteProbabilityParentOption>(entity =>
        {
            entity.HasKey(e => new { e.DiscreteProbabilityId, e.ParentOptionId });

            entity.HasOne(e => e.DiscreteProbability)
                .WithMany(d => d.ParentOptions)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DiscreteUtility>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ValueMetricId);
            entity.HasIndex(e => e.UtilityId);
            entity.Property(e => e.UtilityValue).HasPrecision(DomainConstants.FloatPrecision);

            entity.HasOne(e => e.ValueMetric)
                .WithMany()
                .HasForeignKey(e => e.ValueMetricId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Utility)
                .WithMany(e => e.DiscreteUtilities)
                .HasForeignKey(e => e.UtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ParentOutcomes)
                .WithOne(e => e.DiscreteUtility)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ParentOptions)
                .WithOne(e => e.DiscreteUtility)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscreteUtilityParentOutcome>(entity =>
        {
            entity.HasKey(e => new { e.DiscreteUtilityId, e.ParentOutcomeId });

            entity.HasOne(e => e.DiscreteUtility)
                .WithMany(d => d.ParentOutcomes)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DiscreteUtilityParentOption>(entity =>
        {
            entity.HasKey(e => new { e.DiscreteUtilityId, e.ParentOptionId });

            entity.HasOne(e => e.DiscreteUtility)
                .WithMany(d => d.ParentOptions)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ValueMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<Strategy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Description).HasMaxLength(DomainConstants.MaxLongStringLength);
            entity.Property(e => e.Rationale).HasMaxLength(DomainConstants.MaxLongStringLength);

            entity.HasMany(e => e.StrategyOptions)
                .WithOne(e => e.Strategy)
                .HasForeignKey(e => e.StrategyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StrategyOption>(entity =>
        {
            entity.HasKey(e => new { e.StrategyId, e.OptionId });

            entity.HasOne(e => e.Option)
                .WithMany(e => e.StrategyOptions)
                .HasForeignKey(e => e.OptionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Objective>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Description).HasMaxLength(DomainConstants.MaxLongStringLength);
        });

        modelBuilder.Entity<ProjectRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Role).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<ValueMetric>().HasData(new ValueMetric
        {
            Id = DomainConstants.DefaultValueMetricId,
            Name = DomainConstants.DefaultValueMetricName,
            CreatedAt = new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc).AddTicks(1), TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc).AddTicks(2), TimeSpan.Zero)
        });
        modelBuilder.Entity<Assessment>(entity =>
        {
            entity.ToTable("Assessments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.DecisionQualityAssessments)
                .WithOne(e => e.Assessment)
                .HasForeignKey(e => e.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DecisionQualityAssessment>(entity =>
        {
            entity.ToTable("DecisionQualityAssessments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).HasMaxLength(DomainConstants.MaxLongStringLength);
            entity.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<BoardNode>(entity =>
        {
            entity.ToTable("BoardNode");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Color).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

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
        InvalidateCache();
        var res = await base.SaveChangesAsync(cancellationToken);
        return res;
    }

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

    public void InvalidateCache()
    {
        InvalidateAssessmentsCache();
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
        foreach (var entry in projectRolesEntries)
        {
            // if the user role is being modified or deleted, 
            // we need to invalidate the cache for that user and the project
            _cache.Remove(entry.Entity.UserId.ToString());
            affectedProjectIds.Add(entry.Entity.ProjectId);
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

        foreach (var projectId in affectedProjectIds)
        {
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetInfluenceDiagramKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetIssuesInProjectKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetEdgesInProjectKey(projectId) });
            _cache.InvalidateCacheEntry(new CacheItem{ CacheKey = CacheKeys.GetNodesInProjectKey(projectId) });
        }
    }

    public async Task<int> SaveChangesWhileDuplicatingAsync(CancellationToken cancellationToken = default)
    {
        // no need to invalidate cache here since the duplicated entities will have new ids 
        // and won't affect existing cache entries
        return await base.SaveChangesAsync(cancellationToken);
    }

    //private static void UpdateTimestamps(object sender, EntityEntryEventArgs e)
    //{
    //    // https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/events
    //    if (e.Entry.Entity is BaseEntity entityWithTimestamps)
    //    {
    //        switch (e.Entry.State)
    //        {
    //            case EntityState.Modified:
    //                entityWithTimestamps.UpdatedAt = DateTime.UtcNow;
    //                break;
    //            case EntityState.Added:
    //                entityWithTimestamps.CreatedAt = DateTime.UtcNow;
    //                break;
    //        }
    //    }
    //}

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
