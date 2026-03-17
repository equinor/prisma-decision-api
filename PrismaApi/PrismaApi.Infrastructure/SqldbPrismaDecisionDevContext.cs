using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PrismaApi.Infrastructure;

public partial class SqldbPrismaDecisionDevContext : DbContext
{
    public SqldbPrismaDecisionDevContext()
    {
    }

    public SqldbPrismaDecisionDevContext(DbContextOptions<SqldbPrismaDecisionDevContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AlembicVersion> AlembicVersions { get; set; }

    public virtual DbSet<Decision> Decisions { get; set; }

    public virtual DbSet<DiscreteProbability> DiscreteProbabilities { get; set; }

    public virtual DbSet<DiscreteUtility> DiscreteUtilities { get; set; }

    public virtual DbSet<Edge> Edges { get; set; }

    public virtual DbSet<Issue> Issues { get; set; }

    public virtual DbSet<Node> Nodes { get; set; }

    public virtual DbSet<NodeStyle> NodeStyles { get; set; }

    public virtual DbSet<Objective> Objectives { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<Outcome> Outcomes { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectRole> ProjectRoles { get; set; }

    public virtual DbSet<Strategy> Strategies { get; set; }

    public virtual DbSet<Uncertainty> Uncertainties { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Utility> Utilities { get; set; }

    public virtual DbSet<ValueMetric> ValueMetrics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:sql-prisma-decision-dev.database.windows.net,1433;Initial Catalog=sqldb-prisma-decision-dev;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlembicVersion>(entity =>
        {
            entity.HasKey(e => e.VersionNum).HasName("alembic_version_pkc");

            entity.ToTable("alembic_version");

            entity.Property(e => e.VersionNum)
                .HasMaxLength(32)
                .IsUnicode(false)
                .HasColumnName("version_num");
        });

        modelBuilder.Entity<Decision>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_decision");

            entity.ToTable("decision");

            entity.HasIndex(e => e.IssueId, "ix_decision_issue_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IssueId).HasColumnName("issue_id");
            entity.Property(e => e.Type)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Issue).WithMany(p => p.Decisions)
                .HasForeignKey(d => d.IssueId)
                .HasConstraintName("fk_decision_issue_id_issue");
        });

        modelBuilder.Entity<DiscreteProbability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_discrete_probability");

            entity.ToTable("discrete_probability");

            entity.HasIndex(e => e.OutcomeId, "ix_discrete_probability_outcome_id");

            entity.HasIndex(e => e.UncertaintyId, "ix_discrete_probability_uncertainty_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.OutcomeId).HasColumnName("outcome_id");
            entity.Property(e => e.Probability).HasColumnName("probability");
            entity.Property(e => e.UncertaintyId).HasColumnName("uncertainty_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Outcome).WithMany(p => p.DiscreteProbabilities)
                .HasForeignKey(d => d.OutcomeId)
                .HasConstraintName("fk_discrete_probability_outcome_id_outcome");

            entity.HasOne(d => d.Uncertainty).WithMany(p => p.DiscreteProbabilities)
                .HasForeignKey(d => d.UncertaintyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_discrete_probability_uncertainty_id_uncertainty");

            entity.HasMany(d => d.ParentOptions).WithMany(p => p.DiscreteProbabilities)
                .UsingEntity<Dictionary<string, object>>(
                    "DiscreteProbabilityParentOption",
                    r => r.HasOne<Option>().WithMany()
                        .HasForeignKey("ParentOptionId")
                        .HasConstraintName("fk_discrete_probability_parent_option_parent_option_id_option"),
                    l => l.HasOne<DiscreteProbability>().WithMany()
                        .HasForeignKey("DiscreteProbabilityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_discrete_probability_parent_option_discrete_probability_id_discrete_probability"),
                    j =>
                    {
                        j.HasKey("DiscreteProbabilityId", "ParentOptionId").HasName("pk_discrete_probability_parent_option");
                        j.ToTable("discrete_probability_parent_option");
                        j.IndexerProperty<Guid>("DiscreteProbabilityId").HasColumnName("discrete_probability_id");
                        j.IndexerProperty<Guid>("ParentOptionId").HasColumnName("parent_option_id");
                    });

            entity.HasMany(d => d.ParentOutcomes).WithMany(p => p.DiscreteProbabilitiesNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "DiscreteProbabilityParentOutcome",
                    r => r.HasOne<Outcome>().WithMany()
                        .HasForeignKey("ParentOutcomeId")
                        .HasConstraintName("fk_discrete_probability_parent_outcome_parent_outcome_id_outcome"),
                    l => l.HasOne<DiscreteProbability>().WithMany()
                        .HasForeignKey("DiscreteProbabilityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_discrete_probability_parent_outcome_discrete_probability_id_discrete_probability"),
                    j =>
                    {
                        j.HasKey("DiscreteProbabilityId", "ParentOutcomeId").HasName("pk_discrete_probability_parent_outcome");
                        j.ToTable("discrete_probability_parent_outcome");
                        j.IndexerProperty<Guid>("DiscreteProbabilityId").HasColumnName("discrete_probability_id");
                        j.IndexerProperty<Guid>("ParentOutcomeId").HasColumnName("parent_outcome_id");
                    });
        });

        modelBuilder.Entity<DiscreteUtility>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_discrete_utility");

            entity.ToTable("discrete_utility");

            entity.HasIndex(e => e.UtilityId, "ix_discrete_utility_utility_id");

            entity.HasIndex(e => e.ValueMetricId, "ix_discrete_utility_value_metric_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UtilityId).HasColumnName("utility_id");
            entity.Property(e => e.UtilityValue).HasColumnName("utility_value");
            entity.Property(e => e.ValueMetricId).HasColumnName("value_metric_id");

            entity.HasOne(d => d.Utility).WithMany(p => p.DiscreteUtilities)
                .HasForeignKey(d => d.UtilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_discrete_utility_utility_id_utility");

            entity.HasOne(d => d.ValueMetric).WithMany(p => p.DiscreteUtilities)
                .HasForeignKey(d => d.ValueMetricId)
                .HasConstraintName("fk_discrete_utility_value_metric_id_value_metric");

            entity.HasMany(d => d.ParentOptions).WithMany(p => p.DiscreteUtilities)
                .UsingEntity<Dictionary<string, object>>(
                    "DiscreteUtilityParentOption",
                    r => r.HasOne<Option>().WithMany()
                        .HasForeignKey("ParentOptionId")
                        .HasConstraintName("fk_discrete_utility_parent_option_parent_option_id_option"),
                    l => l.HasOne<DiscreteUtility>().WithMany()
                        .HasForeignKey("DiscreteUtilityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_discrete_utility_parent_option_discrete_utility_id_discrete_utility"),
                    j =>
                    {
                        j.HasKey("DiscreteUtilityId", "ParentOptionId").HasName("pk_discrete_utility_parent_option");
                        j.ToTable("discrete_utility_parent_option");
                        j.IndexerProperty<Guid>("DiscreteUtilityId").HasColumnName("discrete_utility_id");
                        j.IndexerProperty<Guid>("ParentOptionId").HasColumnName("parent_option_id");
                    });

            entity.HasMany(d => d.ParentOutcomes).WithMany(p => p.DiscreteUtilities)
                .UsingEntity<Dictionary<string, object>>(
                    "DiscreteUtilityParentOutcome",
                    r => r.HasOne<Outcome>().WithMany()
                        .HasForeignKey("ParentOutcomeId")
                        .HasConstraintName("fk_discrete_utility_parent_outcome_parent_outcome_id_outcome"),
                    l => l.HasOne<DiscreteUtility>().WithMany()
                        .HasForeignKey("DiscreteUtilityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_discrete_utility_parent_outcome_discrete_utility_id_discrete_utility"),
                    j =>
                    {
                        j.HasKey("DiscreteUtilityId", "ParentOutcomeId").HasName("pk_discrete_utility_parent_outcome");
                        j.ToTable("discrete_utility_parent_outcome");
                        j.IndexerProperty<Guid>("DiscreteUtilityId").HasColumnName("discrete_utility_id");
                        j.IndexerProperty<Guid>("ParentOutcomeId").HasColumnName("parent_outcome_id");
                    });
        });

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_edge");

            entity.ToTable("edge");

            entity.HasIndex(e => e.HeadId, "ix_edge_head_id");

            entity.HasIndex(e => e.ProjectId, "ix_edge_project_id");

            entity.HasIndex(e => e.TailId, "ix_edge_tail_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.HeadId).HasColumnName("head_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.TailId).HasColumnName("tail_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Head).WithMany(p => p.EdgeHeads)
                .HasForeignKey(d => d.HeadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_edge_head_id_node");

            entity.HasOne(d => d.Project).WithMany(p => p.Edges)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_edge_project_id_project");

            entity.HasOne(d => d.Tail).WithMany(p => p.EdgeTails)
                .HasForeignKey(d => d.TailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_edge_tail_id_node");
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_issue");

            entity.ToTable("issue");

            entity.HasIndex(e => e.Name, "ix_issue_name");

            entity.HasIndex(e => e.ProjectId, "ix_issue_project_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Boundary)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("boundary");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedById).HasColumnName("created_by_id");
            entity.Property(e => e.Description)
                .HasMaxLength(6000)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Type)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedById).HasColumnName("updated_by_id");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.IssueCreatedBies)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_issue_created_by_id_user");

            entity.HasOne(d => d.Project).WithMany(p => p.Issues)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_issue_project_id_project");

            entity.HasOne(d => d.UpdatedBy).WithMany(p => p.IssueUpdatedBies)
                .HasForeignKey(d => d.UpdatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_issue_updated_by_id_user");
        });

        modelBuilder.Entity<Node>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_node");

            entity.ToTable("node");

            entity.HasIndex(e => e.IssueId, "ix_node_issue_id");

            entity.HasIndex(e => e.Name, "ix_node_name");

            entity.HasIndex(e => e.ProjectId, "ix_node_project_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IssueId).HasColumnName("issue_id");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Issue).WithMany(p => p.Nodes)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_node_issue_id_issue");

            entity.HasOne(d => d.Project).WithMany(p => p.Nodes)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_node_project_id_project");
        });

        modelBuilder.Entity<NodeStyle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_node_style");

            entity.ToTable("node_style");

            entity.HasIndex(e => e.NodeId, "ix_node_style_node_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.NodeId).HasColumnName("node_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.XPosition).HasColumnName("x_position");
            entity.Property(e => e.YPosition).HasColumnName("y_position");

            entity.HasOne(d => d.Node).WithMany(p => p.NodeStyles)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_node_style_node_id_node");
        });

        modelBuilder.Entity<Objective>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_objective");

            entity.ToTable("objective");

            entity.HasIndex(e => e.Name, "ix_objective_name");

            entity.HasIndex(e => e.ProjectId, "ix_objective_project_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedById).HasColumnName("created_by_id");
            entity.Property(e => e.Description)
                .HasMaxLength(6000)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Type)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedById).HasColumnName("updated_by_id");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.ObjectiveCreatedBies)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_objective_created_by_id_user");

            entity.HasOne(d => d.Project).WithMany(p => p.Objectives)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_objective_project_id_project");

            entity.HasOne(d => d.UpdatedBy).WithMany(p => p.ObjectiveUpdatedBies)
                .HasForeignKey(d => d.UpdatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_objective_updated_by_id_user");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_option");

            entity.ToTable("option");

            entity.HasIndex(e => e.DecisionId, "ix_option_decision_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DecisionId).HasColumnName("decision_id");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Utility).HasColumnName("utility");

            entity.HasOne(d => d.Decision).WithMany(p => p.Options)
                .HasForeignKey(d => d.DecisionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_option_decision_id_decision");
        });

        modelBuilder.Entity<Outcome>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_outcome");

            entity.ToTable("outcome");

            entity.HasIndex(e => e.UncertaintyId, "ix_outcome_uncertainty_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.UncertaintyId).HasColumnName("uncertainty_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Utility).HasColumnName("utility");

            entity.HasOne(d => d.Uncertainty).WithMany(p => p.Outcomes)
                .HasForeignKey(d => d.UncertaintyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_outcome_uncertainty_id_uncertainty");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_project");

            entity.ToTable("project");

            entity.HasIndex(e => e.Name, "ix_project_name");

            entity.HasIndex(e => e.ParentProjectId, "ix_project_parent_project_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedById).HasColumnName("created_by_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.OpportunityStatement)
                .HasMaxLength(6000)
                .IsUnicode(false)
                .HasColumnName("opportunity_statement");
            entity.Property(e => e.ParentProjectId).HasColumnName("parent_project_id");
            entity.Property(e => e.ParentProjectName)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("parent_project_name");
            entity.Property(e => e.Public).HasColumnName("public");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedById).HasColumnName("updated_by_id");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.ProjectCreatedBies)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_project_created_by_id_user");

            entity.HasOne(d => d.UpdatedBy).WithMany(p => p.ProjectUpdatedBies)
                .HasForeignKey(d => d.UpdatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_project_updated_by_id_user");
        });

        modelBuilder.Entity<ProjectRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_project_role");

            entity.ToTable("project_role");

            entity.HasIndex(e => e.ProjectId, "ix_project_role_project_id");

            entity.HasIndex(e => e.UserId, "ix_project_role_user_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedById).HasColumnName("created_by_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Role)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedById).HasColumnName("updated_by_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.ProjectRoleCreatedBies)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_project_role_created_by_id_user");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectRoles)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_project_role_project_id_project");

            entity.HasOne(d => d.UpdatedBy).WithMany(p => p.ProjectRoleUpdatedBies)
                .HasForeignKey(d => d.UpdatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_project_role_updated_by_id_user");

            entity.HasOne(d => d.User).WithMany(p => p.ProjectRoleUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_project_role_user_id_user");
        });

        modelBuilder.Entity<Strategy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_strategy");

            entity.ToTable("strategy");

            entity.HasIndex(e => e.Id, "ix_strategy_id");

            entity.HasIndex(e => e.Name, "ix_strategy_name");

            entity.HasIndex(e => e.ProjectId, "ix_strategy_project_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedById).HasColumnName("created_by_id");
            entity.Property(e => e.Description)
                .HasMaxLength(6000)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Icon)
                .HasMaxLength(6000)
                .IsUnicode(false)
                .HasDefaultValue("")
                .HasColumnName("icon");
            entity.Property(e => e.IconColor)
                .HasMaxLength(6000)
                .IsUnicode(false)
                .HasDefaultValue("")
                .HasColumnName("icon_color");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Rationale)
                .HasMaxLength(6000)
                .IsUnicode(false)
                .HasColumnName("rationale");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UpdatedById).HasColumnName("updated_by_id");

            entity.HasOne(d => d.CreatedBy).WithMany(p => p.StrategyCreatedBies)
                .HasForeignKey(d => d.CreatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_strategy_created_by_id_user");

            entity.HasOne(d => d.Project).WithMany(p => p.Strategies)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_strategy_project_id_project");

            entity.HasOne(d => d.UpdatedBy).WithMany(p => p.StrategyUpdatedBies)
                .HasForeignKey(d => d.UpdatedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_strategy_updated_by_id_user");

            entity.HasMany(d => d.Options).WithMany(p => p.Strategies)
                .UsingEntity<Dictionary<string, object>>(
                    "StrategyOption",
                    r => r.HasOne<Option>().WithMany()
                        .HasForeignKey("OptionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_strategy_option_option_id_option"),
                    l => l.HasOne<Strategy>().WithMany()
                        .HasForeignKey("StrategyId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_strategy_option_strategy_id_strategy"),
                    j =>
                    {
                        j.HasKey("StrategyId", "OptionId").HasName("pk_strategy_option");
                        j.ToTable("strategy_option");
                        j.HasIndex(new[] { "OptionId" }, "ix_strategy_option_option_id");
                        j.HasIndex(new[] { "StrategyId" }, "ix_strategy_option_strategy_id");
                        j.IndexerProperty<Guid>("StrategyId").HasColumnName("strategy_id");
                        j.IndexerProperty<Guid>("OptionId").HasColumnName("option_id");
                    });
        });

        modelBuilder.Entity<Uncertainty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_uncertainty");

            entity.ToTable("uncertainty");

            entity.HasIndex(e => e.IssueId, "ix_uncertainty_issue_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsKey).HasColumnName("is_key");
            entity.Property(e => e.IssueId).HasColumnName("issue_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Issue).WithMany(p => p.Uncertainties)
                .HasForeignKey(d => d.IssueId)
                .HasConstraintName("fk_uncertainty_issue_id_issue");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_user");

            entity.ToTable("user");

            entity.HasIndex(e => e.AzureId, "uq_user_azure_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AzureId)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("azure_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Utility>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_utility");

            entity.ToTable("utility");

            entity.HasIndex(e => e.IssueId, "ix_utility_issue_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IssueId).HasColumnName("issue_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Issue).WithMany(p => p.Utilities)
                .HasForeignKey(d => d.IssueId)
                .HasConstraintName("fk_utility_issue_id_issue");
        });

        modelBuilder.Entity<ValueMetric>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_value_metric");

            entity.ToTable("value_metric");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Name)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
