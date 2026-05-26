using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalProjConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Phase 1: Add columns as nullable — existing rows get NULL, no constraint violation
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Decisions", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Options", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Uncertainties", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Utilities", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Outcomes", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "DiscreteProbabilities", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "DiscreteUtilities", type: "uniqueidentifier", nullable: true);

            // Phase 2: Backfill by joining up the hierarchy to Issues (which already has ProjectId)
            migrationBuilder.Sql(@"
                UPDATE d SET d.ProjectId = i.ProjectId
                FROM Decisions d
                INNER JOIN Issues i ON i.Id = d.IssueId");

            migrationBuilder.Sql(@"
                UPDATE o SET o.ProjectId = i.ProjectId
                FROM Options o
                INNER JOIN Decisions d ON d.Id = o.DecisionId
                INNER JOIN Issues i ON i.Id = d.IssueId");

            migrationBuilder.Sql(@"
                UPDATE u SET u.ProjectId = i.ProjectId
                FROM Uncertainties u
                INNER JOIN Issues i ON i.Id = u.IssueId");

            migrationBuilder.Sql(@"
                UPDATE ut SET ut.ProjectId = i.ProjectId
                FROM Utilities ut
                INNER JOIN Issues i ON i.Id = ut.IssueId");

            migrationBuilder.Sql(@"
                UPDATE o SET o.ProjectId = i.ProjectId
                FROM Outcomes o
                INNER JOIN Uncertainties u ON u.Id = o.UncertaintyId
                INNER JOIN Issues i ON i.Id = u.IssueId");

            migrationBuilder.Sql(@"
                UPDATE dp SET dp.ProjectId = i.ProjectId
                FROM DiscreteProbabilities dp
                INNER JOIN Uncertainties u ON u.Id = dp.UncertaintyId
                INNER JOIN Issues i ON i.Id = u.IssueId");

            migrationBuilder.Sql(@"
                UPDATE du SET du.ProjectId = i.ProjectId
                FROM DiscreteUtilities du
                INNER JOIN Utilities ut ON ut.Id = du.UtilityId
                INNER JOIN Issues i ON i.Id = ut.IssueId");

            // Phase 3: Now safe to constrain NOT NULL — all rows have real ProjectIds
            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Decisions", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Options", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Uncertainties", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Utilities", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Outcomes", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "DiscreteProbabilities", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "DiscreteUtilities", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            // Phase 4: Indexes and FK constraints — data is now valid
            migrationBuilder.CreateIndex(name: "IX_Utilities_ProjectId", table: "Utilities", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_Uncertainties_ProjectId", table: "Uncertainties", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_Outcomes_ProjectId", table: "Outcomes", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_Options_ProjectId", table: "Options", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_DiscreteUtilities_ProjectId", table: "DiscreteUtilities", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_DiscreteProbabilities_ProjectId", table: "DiscreteProbabilities", column: "ProjectId");
            migrationBuilder.CreateIndex(name: "IX_Decisions_ProjectId", table: "Decisions", column: "ProjectId");

            migrationBuilder.AddForeignKey(name: "FK_Decisions_Projects_ProjectId",
                table: "Decisions", column: "ProjectId", principalTable: "Projects",
                principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(name: "FK_DiscreteProbabilities_Projects_ProjectId",
                table: "DiscreteProbabilities", column: "ProjectId", principalTable: "Projects",
                principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(name: "FK_DiscreteUtilities_Projects_ProjectId",
                table: "DiscreteUtilities", column: "ProjectId", principalTable: "Projects",
                principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(name: "FK_Options_Projects_ProjectId",
                table: "Options", column: "ProjectId", principalTable: "Projects",
                principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(name: "FK_Outcomes_Projects_ProjectId",
                table: "Outcomes", column: "ProjectId", principalTable: "Projects",
                principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(name: "FK_Uncertainties_Projects_ProjectId",
                table: "Uncertainties", column: "ProjectId", principalTable: "Projects",
                principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(name: "FK_Utilities_Projects_ProjectId",
                table: "Utilities", column: "ProjectId", principalTable: "Projects",
                principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_Projects_ProjectId",
                table: "Decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscreteProbabilities_Projects_ProjectId",
                table: "DiscreteProbabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscreteUtilities_Projects_ProjectId",
                table: "DiscreteUtilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Options_Projects_ProjectId",
                table: "Options");

            migrationBuilder.DropForeignKey(
                name: "FK_Outcomes_Projects_ProjectId",
                table: "Outcomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Uncertainties_Projects_ProjectId",
                table: "Uncertainties");

            migrationBuilder.DropForeignKey(
                name: "FK_Utilities_Projects_ProjectId",
                table: "Utilities");

            migrationBuilder.DropIndex(
                name: "IX_Utilities_ProjectId",
                table: "Utilities");

            migrationBuilder.DropIndex(
                name: "IX_Uncertainties_ProjectId",
                table: "Uncertainties");

            migrationBuilder.DropIndex(
                name: "IX_Outcomes_ProjectId",
                table: "Outcomes");

            migrationBuilder.DropIndex(
                name: "IX_Options_ProjectId",
                table: "Options");

            migrationBuilder.DropIndex(
                name: "IX_DiscreteUtilities_ProjectId",
                table: "DiscreteUtilities");

            migrationBuilder.DropIndex(
                name: "IX_DiscreteProbabilities_ProjectId",
                table: "DiscreteProbabilities");

            migrationBuilder.DropIndex(
                name: "IX_Decisions_ProjectId",
                table: "Decisions");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Utilities");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Uncertainties");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Outcomes");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Options");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "DiscreteUtilities");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "DiscreteProbabilities");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Decisions");
        }
    }
}
