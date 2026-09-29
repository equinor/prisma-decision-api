using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStakeholderMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StakeholderMatrixes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StakeholderName = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StakeholderRole = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AffectingTheDecision = table.Column<int>(type: "INTEGER", nullable: false),
                    AffectedByTheDecision = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId1 = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderMatrixes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderMatrixes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StakeholderMatrixes_Projects_ProjectId1",
                        column: x => x.ProjectId1,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StakeholderMatrixes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StakeholderMatrixes_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderMatrixes_CreatedById",
                table: "StakeholderMatrixes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderMatrixes_ProjectId",
                table: "StakeholderMatrixes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderMatrixes_ProjectId1",
                table: "StakeholderMatrixes",
                column: "ProjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderMatrixes_UpdatedById",
                table: "StakeholderMatrixes",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StakeholderMatrixes");
        }
    }
}
