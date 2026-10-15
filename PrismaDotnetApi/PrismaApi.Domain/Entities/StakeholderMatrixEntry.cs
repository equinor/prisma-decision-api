using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Domain.Entities
{
    public class StakeholderMatrixEntry : AuditableEntity, IBaseEntity<Guid>, IEntityHandlingPolicy
    {
        public required Guid Id { get; set; }
        public required Guid ProjectId { get; set; }
        public required string StakeholderName { get; set; }
        public required string StakeholderRole { get; set; }
        public required double AffectingTheDecision { get; set; }
        public required double AffectedByTheDecision { get; set; }
        public Project? Project { get; set; }
        [NotMapped]
        public TransferBehavior IsTransferable => TransferBehavior.NotTransferable;

        public static void OnModelConfiguring(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StakeholderMatrixEntry>(entity =>
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
                    .WithMany(project => project.StakeholderMatrices)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


        }
    }
}
