using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteForOptionInStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StrategyOptions_Options_OptionId",
                table: "StrategyOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_StrategyOptions_Options_OptionId",
                table: "StrategyOptions",
                column: "OptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StrategyOptions_Options_OptionId",
                table: "StrategyOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_StrategyOptions_Options_OptionId",
                table: "StrategyOptions",
                column: "OptionId",
                principalTable: "Options",
                principalColumn: "Id");
        }
    }
}
