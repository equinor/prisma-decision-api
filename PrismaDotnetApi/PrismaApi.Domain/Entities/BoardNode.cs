using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class BoardNode : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public string Type { get; set; } = string.Empty;
    public double Height { get; set; }
    public double Width { get; set; }
    public double XPosition { get; set; }
    public double YPosition { get; set; }
    public double Rotation { get; set; }
    public string Data { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public float StrokeWidth { get; set; } = DomainConstants.DefaultStrokeWidth;
    public string StrokeStyle { get; set; } = BoardNodeStrokeStyles.Solid.ToString();
    public int Opacity { get; set; } = DomainConstants.MaxOpacity;
    public int TextSize { get; set; } = DomainConstants.DefaultTextSize;
    public Project? Project { get; set; }
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardNode>(entity =>
        {
            entity.ToTable("BoardNode");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Color).HasMaxLength(DomainConstants.MaxShortStringLength);
            entity.Property(e => e.Opacity)
                .HasDefaultValue(DomainConstants.MaxOpacity);
            entity.Property(e => e.StrokeStyle)
                .HasDefaultValue(BoardNodeStrokeStyles.Solid.ToString());
            entity.Property(e => e.StrokeWidth)
                .HasDefaultValue(DomainConstants.DefaultStrokeWidth);
            entity.Property(e => e.TextSize)
                .HasDefaultValue(DomainConstants.DefaultTextSize);
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
}
