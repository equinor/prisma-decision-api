using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjIdDecisionAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Phase 1: Add column as nullable
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "DecisionQualityAssessments",
                type: "TEXT",
                nullable: true);

            // Phase 2: Backfill via Assessments (SQLite syntax)
            migrationBuilder.Sql(@"
                UPDATE DecisionQualityAssessments
                SET ProjectId = (
                    SELECT a.ProjectId FROM Assessments a WHERE a.Id = DecisionQualityAssessments.AssessmentId
                )");

            // Phase 3: Constrain to NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                table: "DecisionQualityAssessments",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            // Phase 4: Index and FK
            migrationBuilder.CreateIndex(
                name: "IX_DecisionQualityAssessments_ProjectId",
                table: "DecisionQualityAssessments",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_DecisionQualityAssessments_Projects_ProjectId",
                table: "DecisionQualityAssessments",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DecisionQualityAssessments_Projects_ProjectId",
                table: "DecisionQualityAssessments");

            migrationBuilder.DropIndex(
                name: "IX_DecisionQualityAssessments_ProjectId",
                table: "DecisionQualityAssessments");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "DecisionQualityAssessments");
        }
    }
}
