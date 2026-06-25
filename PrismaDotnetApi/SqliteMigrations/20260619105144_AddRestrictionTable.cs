using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRestrictionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestrictionTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EdgeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestrictionTables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestrictionTables_Edges_EdgeId",
                        column: x => x.EdgeId,
                        principalTable: "Edges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RestrictionTables_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RestrictionTables_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestrictionTables_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestrictionEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RestrictionValue = table.Column<double>(type: "REAL", precision: 53, nullable: false, defaultValue: 1.0),
                    ParentOptionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentOutcomeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChildOptionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ChildOutcomeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RestrictionTableId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentStateId = table.Column<Guid>(type: "TEXT", nullable: false, computedColumnSql: "COALESCE([ParentOptionId], [ParentOutcomeId])", stored: true),
                    ChildStateId = table.Column<Guid>(type: "TEXT", nullable: false, computedColumnSql: "COALESCE([ChildOptionId], [ChildOutcomeId])", stored: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestrictionEntries", x => x.Id);
                    table.CheckConstraint("CK_RestrictionEntry_Child", "([ChildOptionId] IS NULL AND [ChildOutcomeId] IS NOT NULL) OR ([ChildOptionId] IS NOT NULL AND [ChildOutcomeId] IS NULL)");
                    table.CheckConstraint("CK_RestrictionEntry_Parent", "([ParentOptionId] IS NULL AND [ParentOutcomeId] IS NOT NULL) OR ([ParentOptionId] IS NOT NULL AND [ParentOutcomeId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_RestrictionEntries_Options_ChildOptionId",
                        column: x => x.ChildOptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestrictionEntries_Options_ParentOptionId",
                        column: x => x.ParentOptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestrictionEntries_Outcomes_ChildOutcomeId",
                        column: x => x.ChildOutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestrictionEntries_Outcomes_ParentOutcomeId",
                        column: x => x.ParentOutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestrictionEntries_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RestrictionEntries_RestrictionTables_RestrictionTableId",
                        column: x => x.RestrictionTableId,
                        principalTable: "RestrictionTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionEntries_ChildOptionId",
                table: "RestrictionEntries",
                column: "ChildOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionEntries_ChildOutcomeId",
                table: "RestrictionEntries",
                column: "ChildOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionEntries_ParentOptionId",
                table: "RestrictionEntries",
                column: "ParentOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionEntries_ParentOutcomeId",
                table: "RestrictionEntries",
                column: "ParentOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionEntries_ParentStateId_ChildStateId_RestrictionTableId",
                table: "RestrictionEntries",
                columns: new[] { "ParentStateId", "ChildStateId", "RestrictionTableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionEntries_ProjectId",
                table: "RestrictionEntries",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionEntries_RestrictionTableId",
                table: "RestrictionEntries",
                column: "RestrictionTableId");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionTables_CreatedById",
                table: "RestrictionTables",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionTables_EdgeId",
                table: "RestrictionTables",
                column: "EdgeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionTables_ProjectId",
                table: "RestrictionTables",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictionTables_UpdatedById",
                table: "RestrictionTables",
                column: "UpdatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestrictionEntries");

            migrationBuilder.DropTable(
                name: "RestrictionTables");
        }
    }
}
