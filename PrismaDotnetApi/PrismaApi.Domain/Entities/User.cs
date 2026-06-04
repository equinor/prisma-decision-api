using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities;

public class User : BaseEntity, IBaseEntity<string>
{
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
    public static void OnModelConfiguring(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(DomainConstants.MaxShortStringLength);
        });
    }
}
