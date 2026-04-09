using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentAndDQAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Assessments_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assessments_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DecisionQualityAssessments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    appropriate_frame = table.Column<int>(type: "int", nullable: false),
                    trade_off_analysis = table.Column<int>(type: "int", nullable: false),
                    reasoning_correctness = table.Column<int>(type: "int", nullable: false),
                    information_reliability = table.Column<int>(type: "int", nullable: false),
                    commitment_to_action = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    doable_alternatives = table.Column<int>(type: "int", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionQualityAssessments", x => x.id);
                    table.ForeignKey(
                        name: "FK_DecisionQualityAssessments_Assessments_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "Assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DecisionQualityAssessments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DecisionQualityAssessments_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CreatedById",
                table: "Assessments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_project_id",
                table: "Assessments",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_UpdatedById",
                table: "Assessments",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionQualityAssessments_assessment_id",
                table: "DecisionQualityAssessments",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionQualityAssessments_CreatedById",
                table: "DecisionQualityAssessments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionQualityAssessments_UpdatedById",
                table: "DecisionQualityAssessments",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisionQualityAssessments");

            migrationBuilder.DropTable(
                name: "Assessments");
        }
    }
}
