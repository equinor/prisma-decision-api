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
            migrationBuilder.DropForeignKey(
                name: "FK_DecisionQualityAssessments_Projects_ProjectId",
                table: "DecisionQualityAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_Projects_ProjectId",
                table: "Decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscreteProbabilities_Projects_ProjectId",
                table: "DiscreteProbabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscreteUtilities_Projects_ProjectId",
                table: "DiscreteUtilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Options_Projects_ProjectId",
                table: "Options");

            migrationBuilder.DropForeignKey(
                name: "FK_Outcomes_Projects_ProjectId",
                table: "Outcomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Uncertainties_Projects_ProjectId",
                table: "Uncertainties");

            migrationBuilder.DropForeignKey(
                name: "FK_Utilities_Projects_ProjectId",
                table: "Utilities");

            migrationBuilder.AddColumn<int>(
                name: "Opacity",
                table: "BoardNode",
                type: "INTEGER",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "StrokeStyle",
                table: "BoardNode",
                type: "TEXT",
                nullable: false,
                defaultValue: "Solid");

            migrationBuilder.AddColumn<float>(
                name: "StrokeWidth",
                table: "BoardNode",
                type: "REAL",
                nullable: false,
                defaultValue: 8f);

            migrationBuilder.AddForeignKey(
                name: "FK_DecisionQualityAssessments_Projects_ProjectId",
                table: "DecisionQualityAssessments",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Decisions_Projects_ProjectId",
                table: "Decisions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscreteProbabilities_Projects_ProjectId",
                table: "DiscreteProbabilities",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscreteUtilities_Projects_ProjectId",
                table: "DiscreteUtilities",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Projects_ProjectId",
                table: "Options",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Outcomes_Projects_ProjectId",
                table: "Outcomes",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Uncertainties_Projects_ProjectId",
                table: "Uncertainties",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Utilities_Projects_ProjectId",
                table: "Utilities",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DecisionQualityAssessments_Projects_ProjectId",
                table: "DecisionQualityAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Decisions_Projects_ProjectId",
                table: "Decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscreteProbabilities_Projects_ProjectId",
                table: "DiscreteProbabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscreteUtilities_Projects_ProjectId",
                table: "DiscreteUtilities");

            migrationBuilder.DropForeignKey(
                name: "FK_Options_Projects_ProjectId",
                table: "Options");

            migrationBuilder.DropForeignKey(
                name: "FK_Outcomes_Projects_ProjectId",
                table: "Outcomes");

            migrationBuilder.DropForeignKey(
                name: "FK_Uncertainties_Projects_ProjectId",
                table: "Uncertainties");

            migrationBuilder.DropForeignKey(
                name: "FK_Utilities_Projects_ProjectId",
                table: "Utilities");

            migrationBuilder.DropColumn(
                name: "Opacity",
                table: "BoardNode");

            migrationBuilder.DropColumn(
                name: "StrokeStyle",
                table: "BoardNode");

            migrationBuilder.DropColumn(
                name: "StrokeWidth",
                table: "BoardNode");

            migrationBuilder.AddForeignKey(
                name: "FK_DecisionQualityAssessments_Projects_ProjectId",
                table: "DecisionQualityAssessments",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Decisions_Projects_ProjectId",
                table: "Decisions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscreteProbabilities_Projects_ProjectId",
                table: "DiscreteProbabilities",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscreteUtilities_Projects_ProjectId",
                table: "DiscreteUtilities",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Options_Projects_ProjectId",
                table: "Options",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Outcomes_Projects_ProjectId",
                table: "Outcomes",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Uncertainties_Projects_ProjectId",
                table: "Uncertainties",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Utilities_Projects_ProjectId",
                table: "Utilities",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
