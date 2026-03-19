using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ValueMetrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueMetrics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    parent_project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    parent_project_name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    opportunity_statement = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    @public = table.Column<bool>(name: "public", type: "bit", nullable: false),
                    end_date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_Projects_Users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Projects_Users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    boundary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.id);
                    table.ForeignKey(
                        name: "FK_Issues_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Issues_Users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Objectives",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objectives", x => x.id);
                    table.ForeignKey(
                        name: "FK_Objectives_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Objectives_Users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Objectives_Users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRoles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    role = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRoles", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    rationale = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                    icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    icon_color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    created_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    updated_by_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.id);
                    table.ForeignKey(
                        name: "FK_Strategies_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Strategies_Users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Strategies_Users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    issue_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_Decisions_Issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "Issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    issue_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Nodes_Issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "Issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Nodes_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Uncertainties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    issue_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_key = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uncertainties", x => x.id);
                    table.ForeignKey(
                        name: "FK_Uncertainties_Issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "Issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    issue_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilities", x => x.id);
                    table.ForeignKey(
                        name: "FK_Utilities_Issues_issue_id",
                        column: x => x.issue_id,
                        principalTable: "Issues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    decision_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    utility = table.Column<double>(type: "float", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.id);
                    table.ForeignKey(
                        name: "FK_Options_Decisions_decision_id",
                        column: x => x.decision_id,
                        principalTable: "Decisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Edges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tail_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    head_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edges", x => x.id);
                    table.ForeignKey(
                        name: "FK_Edges_Nodes_head_id",
                        column: x => x.head_id,
                        principalTable: "Nodes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Edges_Nodes_tail_id",
                        column: x => x.tail_id,
                        principalTable: "Nodes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Edges_Projects_project_id",
                        column: x => x.project_id,
                        principalTable: "Projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeStyles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    node_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    x_position = table.Column<double>(type: "float", nullable: false),
                    y_position = table.Column<double>(type: "float", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeStyles", x => x.id);
                    table.ForeignKey(
                        name: "FK_NodeStyles_Nodes_node_id",
                        column: x => x.node_id,
                        principalTable: "Nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Outcomes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    uncertainty_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    utility = table.Column<double>(type: "float", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outcomes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Outcomes_Uncertainties_uncertainty_id",
                        column: x => x.uncertainty_id,
                        principalTable: "Uncertainties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscreteUtilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    value_metric_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    utility_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    utility_value = table.Column<double>(type: "float(53)", precision: 53, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteUtilities", x => x.id);
                    table.ForeignKey(
                        name: "FK_DiscreteUtilities_Utilities_utility_id",
                        column: x => x.utility_id,
                        principalTable: "Utilities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_DiscreteUtilities_ValueMetrics_value_metric_id",
                        column: x => x.value_metric_id,
                        principalTable: "ValueMetrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategyOptions",
                columns: table => new
                {
                    strategy_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyOptions", x => new { x.strategy_id, x.option_id });
                    table.ForeignKey(
                        name: "FK_StrategyOptions_Options_option_id",
                        column: x => x.option_id,
                        principalTable: "Options",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_StrategyOptions_Strategies_strategy_id",
                        column: x => x.strategy_id,
                        principalTable: "Strategies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscreteProbabilities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    outcome_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    uncertainty_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    probability = table.Column<double>(type: "float(53)", precision: 53, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteProbabilities", x => x.id);
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilities_Outcomes_outcome_id",
                        column: x => x.outcome_id,
                        principalTable: "Outcomes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilities_Uncertainties_uncertainty_id",
                        column: x => x.uncertainty_id,
                        principalTable: "Uncertainties",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DiscreteUtilityParentOptions",
                columns: table => new
                {
                    discrete_utility_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteUtilityParentOptions", x => new { x.discrete_utility_id, x.parent_option_id });
                    table.ForeignKey(
                        name: "FK_DiscreteUtilityParentOptions_DiscreteUtilities_discrete_utility_id",
                        column: x => x.discrete_utility_id,
                        principalTable: "DiscreteUtilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteUtilityParentOptions_Options_parent_option_id",
                        column: x => x.parent_option_id,
                        principalTable: "Options",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DiscreteUtilityParentOutcomes",
                columns: table => new
                {
                    discrete_utility_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_outcome_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteUtilityParentOutcomes", x => new { x.discrete_utility_id, x.parent_outcome_id });
                    table.ForeignKey(
                        name: "FK_DiscreteUtilityParentOutcomes_DiscreteUtilities_discrete_utility_id",
                        column: x => x.discrete_utility_id,
                        principalTable: "DiscreteUtilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteUtilityParentOutcomes_Outcomes_parent_outcome_id",
                        column: x => x.parent_outcome_id,
                        principalTable: "Outcomes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DiscreteProbabilityParentOptions",
                columns: table => new
                {
                    discrete_probability_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_option_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteProbabilityParentOptions", x => new { x.discrete_probability_id, x.parent_option_id });
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilityParentOptions_DiscreteProbabilities_discrete_probability_id",
                        column: x => x.discrete_probability_id,
                        principalTable: "DiscreteProbabilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilityParentOptions_Options_parent_option_id",
                        column: x => x.parent_option_id,
                        principalTable: "Options",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "DiscreteProbabilityParentOutcomes",
                columns: table => new
                {
                    discrete_probability_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    parent_outcome_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteProbabilityParentOutcomes", x => new { x.discrete_probability_id, x.parent_outcome_id });
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilityParentOutcomes_DiscreteProbabilities_discrete_probability_id",
                        column: x => x.discrete_probability_id,
                        principalTable: "DiscreteProbabilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilityParentOutcomes_Outcomes_parent_outcome_id",
                        column: x => x.parent_outcome_id,
                        principalTable: "Outcomes",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                table: "ValueMetrics",
                columns: new[] { "id", "created_at", "name", "updated_at" },
                values: new object[] { new Guid("288e0811-7ab6-5d24-b80c-9fa925b848a6"), new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Unspecified).AddTicks(1), new TimeSpan(0, 0, 0, 0, 0)), "value", new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Unspecified).AddTicks(2), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_issue_id",
                table: "Decisions",
                column: "issue_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteProbabilities_outcome_id",
                table: "DiscreteProbabilities",
                column: "outcome_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteProbabilities_uncertainty_id",
                table: "DiscreteProbabilities",
                column: "uncertainty_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteProbabilityParentOptions_parent_option_id",
                table: "DiscreteProbabilityParentOptions",
                column: "parent_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteProbabilityParentOutcomes_parent_outcome_id",
                table: "DiscreteProbabilityParentOutcomes",
                column: "parent_outcome_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteUtilities_utility_id",
                table: "DiscreteUtilities",
                column: "utility_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteUtilities_value_metric_id",
                table: "DiscreteUtilities",
                column: "value_metric_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteUtilityParentOptions_parent_option_id",
                table: "DiscreteUtilityParentOptions",
                column: "parent_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteUtilityParentOutcomes_parent_outcome_id",
                table: "DiscreteUtilityParentOutcomes",
                column: "parent_outcome_id");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_head_id",
                table: "Edges",
                column: "head_id");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_project_id",
                table: "Edges",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_tail_id",
                table: "Edges",
                column: "tail_id");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_created_by_id",
                table: "Issues",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_project_id",
                table: "Issues",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_updated_by_id",
                table: "Issues",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_issue_id",
                table: "Nodes",
                column: "issue_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_project_id",
                table: "Nodes",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_NodeStyles_node_id",
                table: "NodeStyles",
                column: "node_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_created_by_id",
                table: "Objectives",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_project_id",
                table: "Objectives",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_updated_by_id",
                table: "Objectives",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_Options_decision_id",
                table: "Options",
                column: "decision_id");

            migrationBuilder.CreateIndex(
                name: "IX_Outcomes_uncertainty_id",
                table: "Outcomes",
                column: "uncertainty_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_created_by_id",
                table: "ProjectRoles",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_project_id",
                table: "ProjectRoles",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_updated_by_id",
                table: "ProjectRoles",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_user_id",
                table: "ProjectRoles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_created_by_id",
                table: "Projects",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_updated_by_id",
                table: "Projects",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_created_by_id",
                table: "Strategies",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_project_id",
                table: "Strategies",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_updated_by_id",
                table: "Strategies",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyOptions_option_id",
                table: "StrategyOptions",
                column: "option_id");

            migrationBuilder.CreateIndex(
                name: "IX_Uncertainties_issue_id",
                table: "Uncertainties",
                column: "issue_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_issue_id",
                table: "Utilities",
                column: "issue_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscreteProbabilityParentOptions");

            migrationBuilder.DropTable(
                name: "DiscreteProbabilityParentOutcomes");

            migrationBuilder.DropTable(
                name: "DiscreteUtilityParentOptions");

            migrationBuilder.DropTable(
                name: "DiscreteUtilityParentOutcomes");

            migrationBuilder.DropTable(
                name: "Edges");

            migrationBuilder.DropTable(
                name: "NodeStyles");

            migrationBuilder.DropTable(
                name: "Objectives");

            migrationBuilder.DropTable(
                name: "ProjectRoles");

            migrationBuilder.DropTable(
                name: "StrategyOptions");

            migrationBuilder.DropTable(
                name: "DiscreteProbabilities");

            migrationBuilder.DropTable(
                name: "DiscreteUtilities");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "Strategies");

            migrationBuilder.DropTable(
                name: "Outcomes");

            migrationBuilder.DropTable(
                name: "Utilities");

            migrationBuilder.DropTable(
                name: "ValueMetrics");

            migrationBuilder.DropTable(
                name: "Decisions");

            migrationBuilder.DropTable(
                name: "Uncertainties");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
