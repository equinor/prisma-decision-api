using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Interfaces;
using PrismaApi.Infrastructure.DiscreteTables;

namespace PrismaApi.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("project");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.ParentProjectName).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.OpportunityStatement).HasMaxLength(DomainConstants.MaxLongStringLength);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

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
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Edges)
                .WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.ToTable("issue");
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
                .OnDelete(DeleteBehavior.Restrict);

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
            entity.ToTable("node");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasOne(e => e.NodeStyle)
                .WithOne(e => e.Node)
                .HasForeignKey<NodeStyle>(e => e.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.HeadEdges)
                .WithOne(e => e.HeadNode)
                .HasForeignKey(e => e.HeadId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TailEdges)
                .WithOne(e => e.TailNode)
                .HasForeignKey(e => e.TailId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NodeStyle>(entity =>
        {
            entity.ToTable("node_style");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.ToTable("edge");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Decision>(entity =>
        {
            entity.ToTable("decision");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasMany(e => e.Options)
                .WithOne(e => e.Decision)
                .HasForeignKey(e => e.DecisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.ToTable("option");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<Outcome>(entity =>
        {
            entity.ToTable("outcome");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<Uncertainty>(entity =>
        {
            entity.ToTable("uncertainty");
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Outcomes)
                .WithOne(e => e.Uncertainty)
                .HasForeignKey(e => e.UncertaintyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Utility>(entity =>
        {
            entity.ToTable("utility");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<DiscreteProbability>(entity =>
        {
            entity.ToTable("discrete_probability");
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
                .OnDelete(DeleteBehavior.Cascade);

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
            entity.ToTable("discrete_probability_parent_outcome");
            entity.HasKey(e => new { e.DiscreteProbabilityId, e.ParentOutcomeId });

            entity.HasOne(e => e.DiscreteProbability)
                .WithMany(d => d.ParentOutcomes)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscreteProbabilityParentOption>(entity =>
        {
            entity.ToTable("discrete_probability_parent_option");
            entity.HasKey(e => new { e.DiscreteProbabilityId, e.ParentOptionId });

            entity.HasOne(e => e.DiscreteProbability)
                .WithMany(d => d.ParentOptions)
                .HasForeignKey(e => e.DiscreteProbabilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscreteUtility>(entity =>
        {
            entity.ToTable("discrete_utility");
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
            entity.ToTable("discrete_utility_parent_outcome");
            entity.HasKey(e => new { e.DiscreteUtilityId, e.ParentOutcomeId });

            entity.HasOne(e => e.DiscreteUtility)
                .WithMany(d => d.ParentOutcomes)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscreteUtilityParentOption>(entity =>
        {
            entity.ToTable("discrete_utility_parent_option");
            entity.HasKey(e => new { e.DiscreteUtilityId, e.ParentOptionId });

            entity.HasOne(e => e.DiscreteUtility)
                .WithMany(d => d.ParentOptions)
                .HasForeignKey(e => e.DiscreteUtilityId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ValueMetric>(entity =>
        {
            entity.ToTable("value_metric");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<Strategy>(entity =>
        {
            entity.ToTable("strategy");
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
            entity.ToTable("strategy_option");
            entity.HasKey(e => new { e.StrategyId, e.OptionId });

            entity.HasOne(e => e.Option)
                .WithMany(e => e.StrategyOptions)
                .HasForeignKey(e => e.OptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Objective>(entity =>
        {
            entity.ToTable("objective");
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
            entity.ToTable("project_role");
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
            entity.ToTable("user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.AzureId).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.HasIndex(e => e.AzureId).IsUnique();
        });

        modelBuilder.Entity<ValueMetric>().HasData(new ValueMetric
        {
            Id = DomainConstants.DefaultValueMetricId,
            Name = DomainConstants.DefaultValueMetricName,
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        await OnProjectDeletedCleanupAsync(cancellationToken);
        await OnIssueDeletedCleanupAsync(cancellationToken);
        await OnStrategyDeletedCleanupAsync(cancellationToken);
        await OnOutcomeDeletedCleanupAsync(cancellationToken);
        await OnOptionDeletedCleanupAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveChangesWhileDuplicatingAsync(CancellationToken cancellationToken = default)
    {
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
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
            }

            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    private async Task OnOutcomeDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedOutcomeIds = GetDeletedEntityIds<Outcome>();

        if (deletedOutcomeIds.Any())
        {
            // Load the parent outcome relationships and the discrete probabilities they reference
            var affectedProbParentOutcomes = await DiscreteProbabilityParentOutcomes
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

    private async Task OnProjectDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedProjectIds = GetDeletedEntityIds<Project>();

        if (!deletedProjectIds.Any())
        {
            return;
        }

        var issueIds = await Issues
            .Where(i => deletedProjectIds.Contains(i.ProjectId))
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        var nodeIds = await Nodes
            .Where(n => deletedProjectIds.Contains(n.ProjectId))
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        var strategyIds = await Strategies
            .Where(s => deletedProjectIds.Contains(s.ProjectId))
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        await Edges
            .Where(e => deletedProjectIds.Contains(e.ProjectId))
            .ExecuteDeleteAsync(cancellationToken);

        await DeleteIssueAsync(issueIds, nodeIds, deleteEdgesByNodeIds: false, cancellationToken);

        await StrategyOptions
            .Where(so => strategyIds.Contains(so.StrategyId))
            .ExecuteDeleteAsync(cancellationToken);

        await Issues
            .Where(i => deletedProjectIds.Contains(i.ProjectId))
            .ExecuteDeleteAsync(cancellationToken);

        await Strategies
            .Where(s => deletedProjectIds.Contains(s.ProjectId))
            .ExecuteDeleteAsync(cancellationToken);

        await Objectives
            .Where(o => deletedProjectIds.Contains(o.ProjectId))
            .ExecuteDeleteAsync(cancellationToken);

        await ProjectRoles
            .Where(pr => deletedProjectIds.Contains(pr.ProjectId))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task OnStrategyDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedStrategyIds = GetDeletedEntityIds<Strategy>();

        if (!deletedStrategyIds.Any())
        {
            return;
        }

        await StrategyOptions
            .Where(so => deletedStrategyIds.Contains(so.StrategyId))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task OnIssueDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedIssueIds = GetDeletedEntityIds<Issue>();

        if (!deletedIssueIds.Any())
        {
            return;
        }

        var nodeIds = await Nodes
            .Where(n => deletedIssueIds.Contains(n.IssueId))
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        await DeleteIssueAsync(deletedIssueIds, nodeIds, deleteEdgesByNodeIds: true, cancellationToken);
    }

    private async Task OnOptionDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedOptionIds = GetDeletedEntityIds<Option>();

        if (deletedOptionIds.Any())
        {
            // Load the parent option relationships and the discrete probabilities they reference
            var affectedProbParentOptions = await DiscreteProbabilityParentOptions
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

            // Load the parent option relationships and the discrete utilities they reference
            var affectedUtilParentOptions = await DiscreteUtilityParentOptions
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

    private async Task DeleteIssueAsync(
        IReadOnlyCollection<Guid> issueIds,
        IReadOnlyCollection<Guid> nodeIds,
        bool deleteEdgesByNodeIds,
        CancellationToken cancellationToken)
    {
        var decisionIds = issueIds.Count == 0
            ? new List<Guid>()
            : await Decisions
                .Where(d => issueIds.Contains(d.IssueId))
                .Select(d => d.Id)
                .ToListAsync(cancellationToken);

        var uncertaintyIds = issueIds.Count == 0
            ? new List<Guid>()
            : await Uncertainties
                .Where(u => issueIds.Contains(u.IssueId))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

        var utilityIds = issueIds.Count == 0
            ? new List<Guid>()
            : await Utilities
                .Where(u => issueIds.Contains(u.IssueId))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

        var optionIds = decisionIds.Count == 0
            ? new List<Guid>()
            : await Options
                .Where(o => decisionIds.Contains(o.DecisionId))
                .Select(o => o.Id)
                .ToListAsync(cancellationToken);

        var outcomeIds = uncertaintyIds.Count == 0
            ? new List<Guid>()
            : await Outcomes
                .Where(o => uncertaintyIds.Contains(o.UncertaintyId))
                .Select(o => o.Id)
                .ToListAsync(cancellationToken);

        var discreteProbabilityIds = (uncertaintyIds.Count == 0 && outcomeIds.Count == 0)
            ? new List<Guid>()
            : await DiscreteProbabilities
                .Where(dp => uncertaintyIds.Contains(dp.UncertaintyId) || outcomeIds.Contains(dp.OutcomeId))
                .Select(dp => dp.Id)
                .ToListAsync(cancellationToken);

        var discreteUtilityIds = utilityIds.Count == 0
            ? new List<Guid>()
            : await DiscreteUtilities
                .Where(du => utilityIds.Contains(du.UtilityId))
                .Select(du => du.Id)
                .ToListAsync(cancellationToken);

        if (deleteEdgesByNodeIds && nodeIds.Count > 0)
        {
            await Edges
                .Where(e => nodeIds.Contains(e.HeadId) || nodeIds.Contains(e.TailId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (nodeIds.Count > 0)
        {
            await NodeStyles
                .Where(ns => nodeIds.Contains(ns.NodeId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (discreteProbabilityIds.Count > 0)
        {
            await DiscreteProbabilityParentOptions
                .Where(dppo => discreteProbabilityIds.Contains(dppo.DiscreteProbabilityId))
                .ExecuteDeleteAsync(cancellationToken);

            await DiscreteProbabilityParentOutcomes
                .Where(dppo => discreteProbabilityIds.Contains(dppo.DiscreteProbabilityId))
                .ExecuteDeleteAsync(cancellationToken);

            await DiscreteProbabilities
                .Where(dp => discreteProbabilityIds.Contains(dp.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (discreteUtilityIds.Count > 0)
        {
            await DiscreteUtilityParentOptions
                .Where(dupo => discreteUtilityIds.Contains(dupo.DiscreteUtilityId))
                .ExecuteDeleteAsync(cancellationToken);

            await DiscreteUtilityParentOutcomes
                .Where(dupo => discreteUtilityIds.Contains(dupo.DiscreteUtilityId))
                .ExecuteDeleteAsync(cancellationToken);

            await DiscreteUtilities
                .Where(du => discreteUtilityIds.Contains(du.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (optionIds.Count > 0)
        {
            await StrategyOptions
                .Where(so => optionIds.Contains(so.OptionId))
                .ExecuteDeleteAsync(cancellationToken);

            await Options
                .Where(o => optionIds.Contains(o.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (outcomeIds.Count > 0)
        {
            await Outcomes
                .Where(o => outcomeIds.Contains(o.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (decisionIds.Count > 0)
        {
            await Decisions
                .Where(d => decisionIds.Contains(d.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (uncertaintyIds.Count > 0)
        {
            await Uncertainties
                .Where(u => uncertaintyIds.Contains(u.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (utilityIds.Count > 0)
        {
            await Utilities
                .Where(u => utilityIds.Contains(u.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        if (nodeIds.Count > 0)
        {
            await Nodes
                .Where(n => nodeIds.Contains(n.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }

    public EntityEntry<IBaseEntity<Guid>> CreateEntryFromCollectionAsAdded(IBaseEntity<Guid> entity)
    {
        Entry(entity).State = EntityState.Added;
        return base.Add(entity);
    }


}
