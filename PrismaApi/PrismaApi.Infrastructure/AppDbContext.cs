using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Interfaces;
using PrismaApi.Infrastructure.DiscreteTables;
using System.ComponentModel;

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
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
            }

            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public EntityEntry<IBaseEntity<Guid>> CreateEntryFromCollectionAsAdded(IBaseEntity<Guid> entity)
    {
        Entry(entity).State = EntityState.Added;
        return base.Add(entity);
    }

    public async Task RebuildTablesAsync()
    {
        var issueIds = DiscreteTableSessionInfo.AffectedIssueIds;
        if (issueIds.Count == 0 || IsDiscreteTableEventDisabled)
        {
            return;
        }

        var issues = await Issues
            .Where(issue => issueIds.Contains(issue.Id))
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.Outcomes)
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.DiscreteProbabilities)
                    .ThenInclude(probability => probability.ParentOutcomes)
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.DiscreteProbabilities)
                    .ThenInclude(probability => probability.ParentOptions)
            .Include(issue => issue.Utility!)
                .ThenInclude(utility => utility.DiscreteUtilities)
                    .ThenInclude(utility => utility.ParentOutcomes)
            .Include(issue => issue.Utility!)
                .ThenInclude(utility => utility.DiscreteUtilities)
                    .ThenInclude(utility => utility.ParentOptions)
            .Include(issue => issue.Node!)
                .ThenInclude(node => node.HeadEdges)
                    .ThenInclude(edge => edge.TailNode!)
                        .ThenInclude(node => node.Issue!)
                            .ThenInclude(parentIssue => parentIssue.Uncertainty!)
                                .ThenInclude(uncertainty => uncertainty.Outcomes)
            .Include(issue => issue.Node!)
                .ThenInclude(node => node.HeadEdges)
                    .ThenInclude(edge => edge.TailNode!)
                        .ThenInclude(node => node.Issue!)
                            .ThenInclude(parentIssue => parentIssue.Decision!)
                                .ThenInclude(decision => decision.Options)
            .ToListAsync();

        foreach (var issue in issues)
        {
            if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty != null)
            {
                try
                {
                    await RebuildUncertaintyTable(issue);
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                    throw;
                }
            }
            else if (IsIssueType(issue.Type, IssueType.Utility) && issue.Utility != null)
            {
                try
                {
                    await RebuildUtilityTable(issue);
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                    throw;
                }
            }
        }

        await SaveChangesAsync();
    }

    private async Task RebuildUncertaintyTable(Issue issue)
    {
        var uncertainty = issue.Uncertainty;
        if (uncertainty == null)
        {
            return;
        }
        RemoveDiscreteProbabilities(uncertainty.DiscreteProbabilities);
        uncertainty.DiscreteProbabilities.Clear();

        var (parentOutcomesList, parentOptionsList) = CollectParents(issue);

        if (parentOutcomesList.Count == 0 && parentOptionsList.Count == 0)
        {
            foreach (var outcome in uncertainty.Outcomes)
            {
                var newEntry = new DiscreteProbability
                {
                    Id = Guid.NewGuid(),
                    OutcomeId = outcome.Id,
                    UncertaintyId = uncertainty.Id,
                    Probability = 0
                };
                //Entry(newEntry).State = EntityState.Added;
                //uncertainty.DiscreteProbabilities.Add(newEntry);
                await DiscreteProbabilities.AddAsync(newEntry);
            }
            return;
        }

        var parentCombinations = BuildCombinations(parentOutcomesList.Concat(parentOptionsList).ToList());
        var allOutcomes = new HashSet<Guid>(parentOutcomesList.SelectMany(list => list));
        var allOptions = new HashSet<Guid>(parentOptionsList.SelectMany(list => list));


        foreach (var outcome in uncertainty.Outcomes)
        {
            foreach (var combination in parentCombinations)
            {
                var parentOutcomeIds = combination.Where(id => allOutcomes.Contains(id)).OrderBy(id => id).ToList();
                var parentOptionIds = combination.Where(id => allOptions.Contains(id)).OrderBy(id => id).ToList();

                var probabilityId = Guid.NewGuid();

                var newEntity = new DiscreteProbability
                {
                    Id = probabilityId,
                    OutcomeId = outcome.Id,
                    UncertaintyId = uncertainty.Id,
                    Probability = 0,
                };
                DiscreteProbabilities.Add(newEntity);


                var parentOptions = parentOptionIds.Select(x =>
                    new DiscreteProbabilityParentOption
                    {
                        ParentOptionId = x,
                        DiscreteProbabilityId = probabilityId,
                    }
                ).ToList();
                
                await DiscreteProbabilityParentOptions.AddRangeAsync(parentOptions);

                var parentOutcomes = parentOutcomeIds.Select(x =>
                    new DiscreteProbabilityParentOutcome
                    {
                        ParentOutcomeId = x,
                        DiscreteProbabilityId = probabilityId,
                    }
                ).ToList();

                await DiscreteProbabilityParentOutcomes.AddRangeAsync(parentOutcomes);
            }
        }
    }

    private async Task RebuildUtilityTable(Issue issue)
    {
        var utility = issue.Utility;
        if (utility == null)
        {
            return;
        }

        var (parentOutcomesList, parentOptionsList) = CollectParents(issue);

        RemoveDiscreteUtilities(utility.DiscreteUtilities);
        utility.DiscreteUtilities.Clear();
        if (parentOutcomesList.Count == 0 && parentOptionsList.Count == 0)
        {
            return;
        }

        var parentCombinations = BuildCombinations(parentOutcomesList.Concat(parentOptionsList).ToList());
        var allOutcomes = new HashSet<Guid>(parentOutcomesList.SelectMany(list => list));
        var allOptions = new HashSet<Guid>(parentOptionsList.SelectMany(list => list));

        var desiredKeys = new HashSet<string>();
        var desiredMap = new Dictionary<string, (List<Guid> ParentOutcomes, List<Guid> ParentOptions)>();

        foreach (var combination in parentCombinations)
        {
            var parentOutcomeIds = combination.Where(id => allOutcomes.Contains(id)).OrderBy(id => id).ToList();
            var parentOptionIds = combination.Where(id => allOptions.Contains(id)).OrderBy(id => id).ToList();
            var utilityId = Guid.NewGuid();

            await DiscreteUtilities
                .AddAsync(new DiscreteUtility
                {
                    Id = utilityId,
                    ValueMetricId = DomainConstants.DefaultValueMetricId,
                    UtilityId = utility.Id,
                    UtilityValue = 0,
                });


            var parentOptions = parentOptionIds.Select(x =>
                new DiscreteUtilityParentOption
                {
                    ParentOptionId = x,
                    DiscreteUtilityId = utilityId,
                }
            ).ToList();

            await DiscreteUtilityParentOptions.AddRangeAsync(parentOptions);

            var parentOutcomes = parentOutcomeIds.Select(x =>
                new DiscreteUtilityParentOutcome
                {
                    ParentOutcomeId = x,
                    DiscreteUtilityId = utilityId,
                }
            ).ToList();

            await DiscreteUtilityParentOutcomes.AddRangeAsync(parentOutcomes);
        }

    }

    private (List<List<Guid>> ParentOutcomes, List<List<Guid>> ParentOptions) CollectParents(Issue issue)
    {
        var parentOutcomesList = new List<List<Guid>>();
        var parentOptionsList = new List<List<Guid>>();

        var edges = issue.Node?.HeadEdges ?? Array.Empty<Edge>();
        var distinctEdges = edges
            .GroupBy(edge => new { edge.TailId, edge.HeadId })
            .Select(group => group.First())
            .ToList();

        foreach (var edge in distinctEdges)
        {
            var parentIssue = edge.TailNode?.Issue;
            if (parentIssue == null || !IsBoundaryInScope(parentIssue.Boundary))
            {
                continue;
            }

            if (IsIssueType(parentIssue.Type, IssueType.Uncertainty) && parentIssue.Uncertainty?.IsKey == true)
            {
                parentOutcomesList.Add(parentIssue.Uncertainty.Outcomes.Select(outcome => outcome.Id).ToList());
            }
            else if (IsIssueType(parentIssue.Type, IssueType.Decision) && IsDecisionFocus(parentIssue.Decision?.Type))
            {
                parentOptionsList.Add(parentIssue.Decision.Options.Select(option => option.Id).ToList());
            }
        }

        return (parentOutcomesList, parentOptionsList);
    }

    private void RemoveDiscreteProbabilities(IEnumerable<DiscreteProbability> probabilities)
    {
        var probabilityIds = probabilities.Select(probability => probability.Id).ToList();
        if (probabilityIds.Count == 0)
        {
            return;
        }

        DiscreteProbabilityParentOutcomes.RemoveRange(
            DiscreteProbabilityParentOutcomes.Where(parent => probabilityIds.Contains(parent.DiscreteProbabilityId)));
        DiscreteProbabilityParentOptions.RemoveRange(
            DiscreteProbabilityParentOptions.Where(parent => probabilityIds.Contains(parent.DiscreteProbabilityId)));
        DiscreteProbabilities.RemoveRange(probabilities);
    }

    private void RemoveDiscreteUtilities(IEnumerable<DiscreteUtility> utilities)
    {
        var utilityIds = utilities.Select(utility => utility.Id).ToList();
        if (utilityIds.Count == 0)
        {
            return;
        }

        DiscreteUtilityParentOutcomes.RemoveRange(
            DiscreteUtilityParentOutcomes.Where(parent => utilityIds.Contains(parent.DiscreteUtilityId)));
        DiscreteUtilityParentOptions.RemoveRange(
            DiscreteUtilityParentOptions.Where(parent => utilityIds.Contains(parent.DiscreteUtilityId)));
        DiscreteUtilities.RemoveRange(utilities);
    }

    private static List<List<Guid>> BuildCombinations(List<List<Guid>> groups)
    {
        var results = new List<List<Guid>> { new List<Guid>() };
        foreach (var group in groups)
        {
            var next = new List<List<Guid>>();
            foreach (var existing in results)
            {
                foreach (var item in group)
                {
                    var combination = new List<Guid>(existing) { item };
                    next.Add(combination);
                }
            }

            results = next;
        }

        return results;
    }

    private static bool IsBoundaryInScope(string? boundary)
    {
        var normalized = Normalize(boundary);
        return normalized == Normalize(Boundary.In.ToString()) ||
               normalized == Normalize(Boundary.On.ToString());
    }

    private static bool IsIssueType(string? value, IssueType type)
    {
        return Normalize(value) == Normalize(type.ToString());
    }

    private static bool IsDecisionFocus(string? value)
    {
        return Normalize(value) == Normalize(DecisionHierarchy.Focus.ToString());
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
}
