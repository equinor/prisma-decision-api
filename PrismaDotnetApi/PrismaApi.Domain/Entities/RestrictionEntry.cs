using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class RestrictionEntry : BaseEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public double RestrictionValue { get; set; } = DomainConstants.DefaultRestrictionValue; 

    // parent option id
    public required Guid? ParentOptionId { get; set; }
    // parent outcome id
    public required Guid? ParentOutcomeId { get; set; }
    // child option id
    public required Guid? ChildOptionId { get; set; }
    // child outcome id
    public required Guid? ChildOutcomeId { get; set; }
    public required Guid RestrictionTableId { get; set; }
    public Project? Project { get; set; } = default;
    public Outcome? ParentOutcome { get; set; } = default;
    public Outcome? ChildOutcome { get; set; } = default;
    public Option? ParentOption { get; set; } = default;
    public Option? ChildOption { get; set; } = default;
    public RestrictionTable? RestrictionTable { get; set; } = default;
    public Guid ParentStateId { get; private set; }
    public Guid ChildStateId { get; private set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RestrictionEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RestrictionValue)
                .HasPrecision(DomainConstants.FloatPrecision)
                .HasDefaultValue(DomainConstants.DefaultRestrictionValue);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.NoAction); // delete project deletes the edges and the restriction tables and thus this as well

            entity.HasOne(e => e.RestrictionTable)
                .WithMany(e => e.RestrictionEntries)
                .HasForeignKey(e => e.RestrictionTableId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentOption)
                .WithMany()
                .HasForeignKey(e => e.ParentOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ChildOption)
                .WithMany()
                .HasForeignKey(e => e.ChildOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ParentOutcome)
                .WithMany()
                .HasForeignKey(e => e.ParentOutcomeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ChildOutcome)
                .WithMany()
                .HasForeignKey(e => e.ChildOutcomeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property<Guid>(e => e.ParentStateId) 
                .HasComputedColumnSql($"COALESCE([{nameof(ParentOptionId)}], [{nameof(ParentOutcomeId)}])", stored: true);

            entity.Property<Guid>(e => e.ChildStateId)
                .HasComputedColumnSql($"COALESCE([{nameof(ChildOptionId)}], [{nameof(ChildOutcomeId)}])", stored: true);

            entity.HasIndex(e => new {e.ParentStateId, e.ChildStateId, e.RestrictionTableId})
                .IsUnique();


            entity.ToTable(t =>
            {
                t.HasCheckConstraint(
                    SqlCustomConstraintNames.RestrictionParentConstraintName,
                    $"([{nameof(ParentOptionId)}] IS NULL AND [{nameof(ParentOutcomeId)}] IS NOT NULL) OR ([{nameof(ParentOptionId)}] IS NOT NULL AND [{nameof(ParentOutcomeId)}] IS NULL)"
                );
                t.HasCheckConstraint(
                    SqlCustomConstraintNames.RestrictionChildConstraintName,
                    $"([{nameof(ChildOptionId)}] IS NULL AND [{nameof(ChildOutcomeId)}] IS NOT NULL) OR ([{nameof(ChildOptionId)}] IS NOT NULL AND [{nameof(ChildOutcomeId)}] IS NULL)"
                );
            });
        });
    }
}