using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeRestrictionEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestrictionEntries_Options_ChildOptionId",
                table: "RestrictionEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RestrictionEntries_Options_ParentOptionId",
                table: "RestrictionEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RestrictionEntries_Outcomes_ChildOutcomeId",
                table: "RestrictionEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RestrictionEntries_Outcomes_ParentOutcomeId",
                table: "RestrictionEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_RestrictionEntries_Options_ChildOptionId",
                table: "RestrictionEntries",
                column: "ChildOptionId",
                principalTable: "Options",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestrictionEntries_Options_ParentOptionId",
                table: "RestrictionEntries",
                column: "ParentOptionId",
                principalTable: "Options",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestrictionEntries_Outcomes_ChildOutcomeId",
                table: "RestrictionEntries",
                column: "ChildOutcomeId",
                principalTable: "Outcomes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestrictionEntries_Outcomes_ParentOutcomeId",
                table: "RestrictionEntries",
                column: "ParentOutcomeId",
                principalTable: "Outcomes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestrictionEntries_Options_ChildOptionId",
                table: "RestrictionEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RestrictionEntries_Options_ParentOptionId",
                table: "RestrictionEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RestrictionEntries_Outcomes_ChildOutcomeId",
                table: "RestrictionEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RestrictionEntries_Outcomes_ParentOutcomeId",
                table: "RestrictionEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_RestrictionEntries_Options_ChildOptionId",
                table: "RestrictionEntries",
                column: "ChildOptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RestrictionEntries_Options_ParentOptionId",
                table: "RestrictionEntries",
                column: "ParentOptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RestrictionEntries_Outcomes_ChildOutcomeId",
                table: "RestrictionEntries",
                column: "ChildOutcomeId",
                principalTable: "Outcomes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RestrictionEntries_Outcomes_ParentOutcomeId",
                table: "RestrictionEntries",
                column: "ParentOutcomeId",
                principalTable: "Outcomes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
