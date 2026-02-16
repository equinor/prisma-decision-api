using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.DiscreteTables;

namespace PrismaApi.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    internal DiscreteTableSessionInfo DiscreteTableSessionInfo { get; } = new();
    internal bool IsDiscreteTableEventDisabled { get; set; }

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
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.ParentProjectName).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.OpportunityStatement).HasMaxLength(DomainConstants.MaxLongStringLength);

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
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Description).HasMaxLength(DomainConstants.MaxLongStringLength);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Boundary).HasMaxLength(DomainConstants.MaxShortStringLength);

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
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TailEdges)
                .WithOne(e => e.TailNode)
                .HasForeignKey(e => e.TailId)
                .OnDelete(DeleteBehavior.Cascade);
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

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscreteProbabilityParentOption>(entity =>
        {
            entity.ToTable("discrete_probability_parent_option");
            entity.HasKey(e => new { e.DiscreteProbabilityId, e.ParentOptionId });

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

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscreteUtilityParentOption>(entity =>
        {
            entity.ToTable("discrete_utility_parent_option");
            entity.HasKey(e => new { e.DiscreteUtilityId, e.ParentOptionId });

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ValueMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<Strategy>(entity =>
        {
            entity.HasKey(e => e.Id);
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
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Objective>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Description).HasMaxLength(DomainConstants.MaxLongStringLength);
        });

        modelBuilder.Entity<ProjectRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(DomainConstants.MaxShortStringLength);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.AzureId).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.HasIndex(e => e.AzureId).IsUnique();
        });

        modelBuilder.Entity<ValueMetric>().HasData(new ValueMetric
        {
            Id = DomainConstants.DefaultValueMetricId,
            Name = "default value metric"
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
