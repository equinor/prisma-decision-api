using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NodesStrokeProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Opacity",
                table: "BoardNode",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "StrokeStyle",
                table: "BoardNode",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "StrokeWidth",
                table: "BoardNode",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Opacity",
                table: "BoardNode");

            migrationBuilder.DropColumn(
                name: "StrokeStyle",
                table: "BoardNode");

            migrationBuilder.DropColumn(
                name: "StrokeWidth",
                table: "BoardNode");
        }
    }
}
