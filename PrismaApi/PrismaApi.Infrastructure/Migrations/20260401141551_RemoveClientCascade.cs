using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClientCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StrategyOptions_Strategies_StrategyId",
                table: "StrategyOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_StrategyOptions_Strategies_StrategyId",
                table: "StrategyOptions",
                column: "StrategyId",
                principalTable: "Strategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StrategyOptions_Strategies_StrategyId",
                table: "StrategyOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_StrategyOptions_Strategies_StrategyId",
                table: "StrategyOptions",
                column: "StrategyId",
                principalTable: "Strategies",
                principalColumn: "Id");
        }
    }
}
