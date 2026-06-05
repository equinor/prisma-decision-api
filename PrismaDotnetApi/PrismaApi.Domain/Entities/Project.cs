using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Project : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentProjectId { get; set; }
    public string? ParentProjectName { get; set; }
    public string OpportunityStatement { get; set; } = string.Empty;
    public bool Public { get; set; }
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow.AddDays(30);

    public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
    public ICollection<Objective> Objectives { get; set; } = new List<Objective>();
    public ICollection<Strategy> Strategies { get; set; } = new List<Strategy>();
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    public ICollection<Node> Nodes { get; set; } = new List<Node>();
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
    public ICollection<BoardNode> BoardNodes { get; set; } = new List<BoardNode>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
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
    }
}
