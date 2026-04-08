using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeUtilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscreteUtilities_Utilities_UtilityId",
                table: "DiscreteUtilities");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscreteUtilities_Utilities_UtilityId",
                table: "DiscreteUtilities",
                column: "UtilityId",
                principalTable: "Utilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscreteUtilities_Utilities_UtilityId",
                table: "DiscreteUtilities");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscreteUtilities_Utilities_UtilityId",
                table: "DiscreteUtilities",
                column: "UtilityId",
                principalTable: "Utilities",
                principalColumn: "Id");
        }
    }
}
