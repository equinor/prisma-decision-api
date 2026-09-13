using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class User : BaseEntity, IBaseEntity<string>, IEntityHandlingPolicy
{
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
    [NotMapped]
    public TransferBehavior IsTransferable => TransferBehavior.NotTransferable;
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = DomainConstants.DeletedUserId,
            Name = DomainConstants.DeletedUserName,
            CreatedAt = new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc).AddTicks(1), TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc).AddTicks(2), TimeSpan.Zero)
        });
    }
}
