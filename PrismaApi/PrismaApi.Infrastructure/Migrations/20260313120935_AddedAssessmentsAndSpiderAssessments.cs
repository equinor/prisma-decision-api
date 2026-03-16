using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedAssessmentsAndSpiderAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assessment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by_id = table.Column<int>(type: "int", nullable: false),
                    updated_by_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assessment", x => x.id);
                    table.ForeignKey(
                        name: "FK_assessment_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assessment_user_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assessment_user_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "spider_assessment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    value = table.Column<int>(type: "int", precision: 53, nullable: false),
                    risk = table.Column<int>(type: "int", precision: 53, nullable: false),
                    cost = table.Column<int>(type: "int", precision: 53, nullable: false),
                    feasibility = table.Column<int>(type: "int", precision: 53, nullable: false),
                    impact = table.Column<int>(type: "int", precision: 53, nullable: false),
                    comment = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    assessment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by_id = table.Column<int>(type: "int", nullable: false),
                    updated_by_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spider_assessment", x => x.id);
                    table.ForeignKey(
                        name: "FK_spider_assessment_assessment_assessment_id",
                        column: x => x.assessment_id,
                        principalTable: "assessment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_spider_assessment_user_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_spider_assessment_user_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assessment_created_by_id",
                table: "assessment",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_project_id",
                table: "assessment",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_assessment_updated_by_id",
                table: "assessment",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_spider_assessment_assessment_id",
                table: "spider_assessment",
                column: "assessment_id");

            migrationBuilder.CreateIndex(
                name: "IX_spider_assessment_created_by_id",
                table: "spider_assessment",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_spider_assessment_updated_by_id",
                table: "spider_assessment",
                column: "updated_by_id");
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
