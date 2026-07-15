using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddboardSheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardNode_Projects_ProjectId",
                table: "BoardNode");

            migrationBuilder.AddColumn<Guid>(
                name: "BoardSheetId",
                table: "BoardNode",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BoardSheet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardSheet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardSheet_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardSheet_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BoardSheet_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
                INSERT INTO BoardSheet (Id, ProjectId, Name, CreatedAt, UpdatedAt, CreatedById, UpdatedById)
                SELECT 
                    lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' || '4' || substr(hex(randomblob(2)), 2) || '-' || substr('89ab', abs(random()) % 4 + 1, 1) || substr(hex(randomblob(2)), 2) || '-' || hex(randomblob(6))) as Id,
                    p.Id as ProjectId,
                    'Default Board Sheet' as Name,
                    datetime('now') as CreatedAt,
                    datetime('now') as UpdatedAt,
                    p.CreatedById as CreatedById,
                    p.UpdatedById as UpdatedById
                FROM Projects p
            ");

            migrationBuilder.Sql(@"
                UPDATE BoardNode
                SET BoardSheetId = (
                    SELECT bs.Id 
                    FROM BoardSheet bs 
                    WHERE bs.ProjectId = BoardNode.ProjectId
                )
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "BoardSheetId",
                table: "BoardNode",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BoardNode_BoardSheetId",
                table: "BoardNode",
                column: "BoardSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardSheet_CreatedById",
                table: "BoardSheet",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BoardSheet_ProjectId",
                table: "BoardSheet",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardSheet_UpdatedById",
                table: "BoardSheet",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardNode_BoardSheet_BoardSheetId",
                table: "BoardNode",
                column: "BoardSheetId",
                principalTable: "BoardSheet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BoardNode_Projects_ProjectId",
                table: "BoardNode",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardNode_BoardSheet_BoardSheetId",
                table: "BoardNode");

            migrationBuilder.DropForeignKey(
                name: "FK_BoardNode_Projects_ProjectId",
                table: "BoardNode");

            migrationBuilder.DropTable(
                name: "BoardSheet");

            migrationBuilder.DropIndex(
                name: "IX_BoardNode_BoardSheetId",
                table: "BoardNode");

            migrationBuilder.DropColumn(
                name: "BoardSheetId",
                table: "BoardNode");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardNode_Projects_ProjectId",
                table: "BoardNode",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
