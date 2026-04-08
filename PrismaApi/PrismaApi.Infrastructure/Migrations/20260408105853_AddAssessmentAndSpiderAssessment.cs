using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentAndSpiderAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assessment",
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
                    table.PrimaryKey("PK_assessment", x => x.id);
                    table.ForeignKey(
                        name: "FK_assessment_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assessment_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assessment_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "spider_assessment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    appropriate_frame = table.Column<int>(type: "int", nullable: false),
                    trade_off_analysis = table.Column<int>(type: "int", nullable: false),
                    reasoning_correctness = table.Column<int>(type: "int", nullable: false),
                    information_reliability = table.Column<int>(type: "int", nullable: false),
                    commitment_to_action = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    doable_alternatives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spider_assessment", x => x.id);
                    table.ForeignKey(
                        name: "FK_spider_assessment_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_spider_assessment_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_spider_assessment_assessment_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_CreatedById",
                table: "assessment",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_project_id",
                table: "assessment",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_UpdatedById",
                table: "assessment",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_spider_assessment_assessment_id",
                table: "spider_assessment",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "IX_spider_assessment_CreatedById",
                table: "spider_assessment",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_spider_assessment_UpdatedById",
                table: "spider_assessment",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "spider_assessment");

            migrationBuilder.DropTable(
                name: "assessment");
        }
    }
}
