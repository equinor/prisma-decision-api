using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities
{
    public class StakeholderMatrix : AuditableEntity, IBaseEntity<Guid>
    {
        public required Guid Id { get; set; }
        public required Guid ProjectId { get; set; }

        public required string StakeholderName { get; set; }
        public required string StakeholderRole { get; set; }
        public required int AffectingTheDecision { get; set; }
        public required int AffectedByTheDecision { get; set; }
        public Project? Project { get; set; }

        public static void OnModelConfiguring(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StakeholderMatrix>(entity =>
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
                entity.Property(e => e.StakeholderName).HasMaxLength(DomainConstants.MaxShortStringLength);
                entity.Property(e => e.StakeholderRole).HasMaxLength(DomainConstants.MaxShortStringLength);
                entity.HasOne(e => e.Project)
                    .WithMany()
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


        }
    }
}
