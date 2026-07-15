using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class RestrictionTable : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public required Guid EdgeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Project? Project { get; set; } = default;
    public Edge? Edge { get; set; } = default;
    public List<RestrictionEntry> RestrictionEntries { get; set; } = [];

    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RestrictionTable>(entity =>
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

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // delete project deletes the edges and thus this as well
            
            entity.HasOne(e => e.Edge)
                .WithOne()
                .HasForeignKey<RestrictionTable>(e => e.EdgeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}