using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class Decision : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid IssueId { get; set; }
    public required Guid ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public Issue? Issue { get; set; }
    public Project? Project { get; set; }
    public ICollection<Option> Options { get; set; } = new List<Option>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Decision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(DomainConstants.MaxShortStringLength);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // Cascade path already exists via Projects -> Issues -> Decisions

            entity.HasMany(e => e.Options)
                .WithOne(e => e.Decision)
                .HasForeignKey(e => e.DecisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
