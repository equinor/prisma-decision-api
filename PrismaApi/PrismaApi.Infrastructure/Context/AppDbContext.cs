using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Interfaces;
using PrismaApi.Infrastructure.DiscreteTables;

namespace PrismaApi.Infrastructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DiscreteTableSessionInfo DiscreteTableSessionInfo { get; } = new();
    public bool IsDiscreteTableEventDisabled { get; set; }

    public DbSet<PrismaApi.Domain.Entities.Project> Projects => Set<PrismaApi.Domain.Entities.Project>();
    public DbSet<PrismaApi.Domain.Entities.Issue> Issues => Set<PrismaApi.Domain.Entities.Issue>();
    public DbSet<PrismaApi.Domain.Entities.Node> Nodes => Set<PrismaApi.Domain.Entities.Node>();
    public DbSet<PrismaApi.Domain.Entities.NodeStyle> NodeStyles => Set<PrismaApi.Domain.Entities.NodeStyle>();
    public DbSet<PrismaApi.Domain.Entities.Edge> Edges => Set<PrismaApi.Domain.Entities.Edge>();
    public DbSet<PrismaApi.Domain.Entities.Decision> Decisions => Set<PrismaApi.Domain.Entities.Decision>();
    public DbSet<PrismaApi.Domain.Entities.Option> Options => Set<PrismaApi.Domain.Entities.Option>();
    public DbSet<PrismaApi.Domain.Entities.Outcome> Outcomes => Set<PrismaApi.Domain.Entities.Outcome>();
    public DbSet<PrismaApi.Domain.Entities.Uncertainty> Uncertainties => Set<PrismaApi.Domain.Entities.Uncertainty>();
    public DbSet<PrismaApi.Domain.Entities.Utility> Utilities => Set<PrismaApi.Domain.Entities.Utility>();
    public DbSet<PrismaApi.Domain.Entities.ValueMetric> ValueMetrics => Set<PrismaApi.Domain.Entities.ValueMetric>();
    public DbSet<PrismaApi.Domain.Entities.DiscreteProbability> DiscreteProbabilities => Set<PrismaApi.Domain.Entities.DiscreteProbability>();
    public DbSet<PrismaApi.Domain.Entities.DiscreteProbabilityParentOutcome> DiscreteProbabilityParentOutcomes => Set<PrismaApi.Domain.Entities.DiscreteProbabilityParentOutcome>();
    public DbSet<PrismaApi.Domain.Entities.DiscreteProbabilityParentOption> DiscreteProbabilityParentOptions => Set<PrismaApi.Domain.Entities.DiscreteProbabilityParentOption>();
    public DbSet<PrismaApi.Domain.Entities.DiscreteUtility> DiscreteUtilities => Set<PrismaApi.Domain.Entities.DiscreteUtility>();
    public DbSet<PrismaApi.Domain.Entities.DiscreteUtilityParentOutcome> DiscreteUtilityParentOutcomes => Set<PrismaApi.Domain.Entities.DiscreteUtilityParentOutcome>();
    public DbSet<PrismaApi.Domain.Entities.DiscreteUtilityParentOption> DiscreteUtilityParentOptions => Set<PrismaApi.Domain.Entities.DiscreteUtilityParentOption>();
    public DbSet<PrismaApi.Domain.Entities.Strategy> Strategies => Set<PrismaApi.Domain.Entities.Strategy>();
    public DbSet<PrismaApi.Domain.Entities.StrategyOption> StrategyOptions => Set<PrismaApi.Domain.Entities.StrategyOption>();
    public DbSet<PrismaApi.Domain.Entities.Objective> Objectives => Set<PrismaApi.Domain.Entities.Objective>();
    public DbSet<PrismaApi.Domain.Entities.ProjectRole> ProjectRoles => Set<PrismaApi.Domain.Entities.ProjectRole>();
    public DbSet<PrismaApi.Domain.Entities.User> Users => Set<PrismaApi.Domain.Entities.User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PrismaApi.Domain.Entities.Project>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.Issue>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.Node>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.NodeStyle>(entity =>
        {
            entity.ToTable("node_style");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.Edge>(entity =>
        {
            entity.ToTable("edge");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.Decision>(entity =>
        {
            entity.ToTable("decision");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasMany(e => e.Options)
                .WithOne(e => e.Decision)
                .HasForeignKey(e => e.DecisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.Option>(entity =>
        {
            entity.ToTable("option");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.Outcome>(entity =>
        {
            entity.ToTable("outcome");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.Uncertainty>(entity =>
        {
            entity.ToTable("uncertainty");
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Outcomes)
                .WithOne(e => e.Uncertainty)
                .HasForeignKey(e => e.UncertaintyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.Utility>(entity =>
        {
            entity.ToTable("utility");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.DiscreteProbability>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.DiscreteProbabilityParentOutcome>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.DiscreteProbabilityParentOption>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.DiscreteUtility>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.DiscreteUtilityParentOutcome>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.DiscreteUtilityParentOption>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.ValueMetric>(entity =>
        {
            entity.ToTable("value_metric");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.Strategy>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.StrategyOption>(entity =>
        {
            entity.ToTable("strategy_option");
            entity.HasKey(e => new { e.StrategyId, e.OptionId });

            entity.HasOne(e => e.Option)
                .WithMany(e => e.StrategyOptions)
                .HasForeignKey(e => e.OptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.Objective>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.ProjectRole>(entity =>
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

        modelBuilder.Entity<PrismaApi.Domain.Entities.User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.AzureId).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.HasIndex(e => e.AzureId).IsUnique();
        });

        modelBuilder.Entity<PrismaApi.Domain.Entities.ValueMetric>().HasData(new ValueMetric
        {
            Id = DomainConstants.DefaultValueMetricId,
            Name = DomainConstants.DefaultValueMetricName,
            CreatedAt = new DateTimeOffset(new DateTime(2026, 3, 16, 13, 51, 24, 68, DateTimeKind.Utc).AddTicks(5473), TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(new DateTime(2026, 3, 16, 13, 51, 24, 68, DateTimeKind.Utc).AddTicks(5475), TimeSpan.Zero)
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
        var deletedOutcomeIds = ChangeTracker.Entries<PrismaApi.Domain.Entities.Outcome>()
        .Where(e => e.State == EntityState.Deleted)
        .Select(e => e.Entity.Id)
        .ToHashSet();

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
                .Cast<PrismaApi.Domain.Entities.DiscreteProbability>()
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
                .Cast<PrismaApi.Domain.Entities.DiscreteUtility>()
                .ToList();

            DiscreteUtilities.RemoveRange(affectedUtils);
        }
    }

    private async Task OnOptionDeletedCleanupAsync(CancellationToken cancellationToken = default)
    {
        var deletedOptionIds = ChangeTracker.Entries<Option>()
        .Where(e => e.State == EntityState.Deleted)
        .Select(e => e.Entity.Id)
        .ToHashSet();

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
                .Cast<PrismaApi.Domain.Entities.DiscreteProbability>()
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
                .Cast<PrismaApi.Domain.Entities.DiscreteUtility>()
                .ToList();

            DiscreteUtilities.RemoveRange(affectedUtils);
        }
    }

    public EntityEntry<IBaseEntity<Guid>> CreateEntryFromCollectionAsAdded(IBaseEntity<Guid> entity)
    {
        Entry(entity).State = EntityState.Added;
        return base.Add(entity);
    }

    
}
