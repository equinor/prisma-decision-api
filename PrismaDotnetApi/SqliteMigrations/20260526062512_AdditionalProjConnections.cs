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
            // Phase 1: Add columns as nullable
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Decisions", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Options", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Uncertainties", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Utilities", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "Outcomes", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "DiscreteProbabilities", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ProjectId", table: "DiscreteUtilities", type: "TEXT", nullable: true);

            // Phase 2: Backfill using correlated subqueries (SQLite syntax)
            migrationBuilder.Sql(@"
                UPDATE Decisions
                SET ProjectId = (
                    SELECT i.ProjectId FROM Issues i WHERE i.Id = Decisions.IssueId
                )");

            migrationBuilder.Sql(@"
                UPDATE Options
                SET ProjectId = (
                    SELECT i.ProjectId FROM Decisions d
                    INNER JOIN Issues i ON i.Id = d.IssueId
                    WHERE d.Id = Options.DecisionId
                )");

            migrationBuilder.Sql(@"
                UPDATE Uncertainties
                SET ProjectId = (
                    SELECT i.ProjectId FROM Issues i WHERE i.Id = Uncertainties.IssueId
                )");

            migrationBuilder.Sql(@"
                UPDATE Utilities
                SET ProjectId = (
                    SELECT i.ProjectId FROM Issues i WHERE i.Id = Utilities.IssueId
                )");

            migrationBuilder.Sql(@"
                UPDATE Outcomes
                SET ProjectId = (
                    SELECT i.ProjectId FROM Uncertainties u
                    INNER JOIN Issues i ON i.Id = u.IssueId
                    WHERE u.Id = Outcomes.UncertaintyId
                )");

            migrationBuilder.Sql(@"
                UPDATE DiscreteProbabilities
                SET ProjectId = (
                    SELECT i.ProjectId FROM Uncertainties u
                    INNER JOIN Issues i ON i.Id = u.IssueId
                    WHERE u.Id = DiscreteProbabilities.UncertaintyId
                )");

            migrationBuilder.Sql(@"
                UPDATE DiscreteUtilities
                SET ProjectId = (
                    SELECT i.ProjectId FROM Utilities ut
                    INNER JOIN Issues i ON i.Id = ut.IssueId
                    WHERE ut.Id = DiscreteUtilities.UtilityId
                )");

            // Phase 3: Constrain to NOT NULL (EF Core rebuilds the SQLite table under the hood)
            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Decisions", type: "TEXT", nullable: false,
                oldClrType: typeof(Guid), oldType: "TEXT", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Options", type: "TEXT", nullable: false,
                oldClrType: typeof(Guid), oldType: "TEXT", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Uncertainties", type: "TEXT", nullable: false,
                oldClrType: typeof(Guid), oldType: "TEXT", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Utilities", type: "TEXT", nullable: false,
                oldClrType: typeof(Guid), oldType: "TEXT", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "Outcomes", type: "TEXT", nullable: false,
                oldClrType: typeof(Guid), oldType: "TEXT", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "DiscreteProbabilities", type: "TEXT", nullable: false,
                oldClrType: typeof(Guid), oldType: "TEXT", oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(name: "ProjectId", table: "DiscreteUtilities", type: "TEXT", nullable: false,
                oldClrType: typeof(Guid), oldType: "TEXT", oldNullable: true);

            // Phase 4: Indexes and FK constraints — identical to before
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
