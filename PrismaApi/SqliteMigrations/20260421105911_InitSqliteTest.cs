using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitSqliteTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValueMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ParentProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentProjectName = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    OpportunityStatement = table.Column<string>(type: "TEXT", maxLength: 6000, nullable: false),
                    Public = table.Column<bool>(type: "INTEGER", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Projects_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assessments_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Boundary = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 6000, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Issues_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Objectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 6000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Objectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Objectives_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Objectives_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Objectives_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 6000, nullable: false),
                    Rationale = table.Column<string>(type: "TEXT", maxLength: 6000, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    IconColor = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Strategies_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Strategies_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Strategies_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DecisionQualityAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppropriateFrame = table.Column<int>(type: "INTEGER", nullable: false),
                    TradeOffAnalysis = table.Column<int>(type: "INTEGER", nullable: false),
                    ReasoningCorrectness = table.Column<int>(type: "INTEGER", nullable: false),
                    InformationReliability = table.Column<int>(type: "INTEGER", nullable: false),
                    CommitmentToAction = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 6000, nullable: false),
                    DoableAlternatives = table.Column<int>(type: "INTEGER", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionQualityAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisionQualityAssessments_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DecisionQualityAssessments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DecisionQualityAssessments_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Decisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Decisions_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Nodes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Uncertainties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsKey = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uncertainties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Uncertainties_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IssueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilities_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DecisionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Utility = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Options_Decisions_DecisionId",
                        column: x => x.DecisionId,
                        principalTable: "Decisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Edges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TailId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Edges_Nodes_HeadId",
                        column: x => x.HeadId,
                        principalTable: "Nodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Edges_Nodes_TailId",
                        column: x => x.TailId,
                        principalTable: "Nodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Edges_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NodeStyles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NodeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    XPosition = table.Column<double>(type: "REAL", nullable: false),
                    YPosition = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeStyles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeStyles_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Outcomes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UncertaintyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Utility = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Outcomes_Uncertainties_UncertaintyId",
                        column: x => x.UncertaintyId,
                        principalTable: "Uncertainties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscreteUtilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValueMetricId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UtilityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UtilityValue = table.Column<double>(type: "REAL", precision: 53, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteUtilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscreteUtilities_Utilities_UtilityId",
                        column: x => x.UtilityId,
                        principalTable: "Utilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteUtilities_ValueMetrics_ValueMetricId",
                        column: x => x.ValueMetricId,
                        principalTable: "ValueMetrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StrategyOptions",
                columns: table => new
                {
                    StrategyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OptionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyOptions", x => new { x.StrategyId, x.OptionId });
                    table.ForeignKey(
                        name: "FK_StrategyOptions_Options_OptionId",
                        column: x => x.OptionId,
                        principalTable: "Options",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StrategyOptions_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscreteProbabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OutcomeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UncertaintyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Probability = table.Column<double>(type: "REAL", precision: 53, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteProbabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilities_Outcomes_OutcomeId",
                        column: x => x.OutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilities_Uncertainties_UncertaintyId",
                        column: x => x.UncertaintyId,
                        principalTable: "Uncertainties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiscreteUtilityParentOptions",
                columns: table => new
                {
                    DiscreteUtilityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentOptionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteUtilityParentOptions", x => new { x.DiscreteUtilityId, x.ParentOptionId });
                    table.ForeignKey(
                        name: "FK_DiscreteUtilityParentOptions_DiscreteUtilities_DiscreteUtilityId",
                        column: x => x.DiscreteUtilityId,
                        principalTable: "DiscreteUtilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteUtilityParentOptions_Options_ParentOptionId",
                        column: x => x.ParentOptionId,
                        principalTable: "Options",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiscreteUtilityParentOutcomes",
                columns: table => new
                {
                    DiscreteUtilityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentOutcomeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteUtilityParentOutcomes", x => new { x.DiscreteUtilityId, x.ParentOutcomeId });
                    table.ForeignKey(
                        name: "FK_DiscreteUtilityParentOutcomes_DiscreteUtilities_DiscreteUtilityId",
                        column: x => x.DiscreteUtilityId,
                        principalTable: "DiscreteUtilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteUtilityParentOutcomes_Outcomes_ParentOutcomeId",
                        column: x => x.ParentOutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiscreteProbabilityParentOptions",
                columns: table => new
                {
                    DiscreteProbabilityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentOptionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteProbabilityParentOptions", x => new { x.DiscreteProbabilityId, x.ParentOptionId });
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilityParentOptions_DiscreteProbabilities_DiscreteProbabilityId",
                        column: x => x.DiscreteProbabilityId,
                        principalTable: "DiscreteProbabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilityParentOptions_Options_ParentOptionId",
                        column: x => x.ParentOptionId,
                        principalTable: "Options",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiscreteProbabilityParentOutcomes",
                columns: table => new
                {
                    DiscreteProbabilityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ParentOutcomeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscreteProbabilityParentOutcomes", x => new { x.DiscreteProbabilityId, x.ParentOutcomeId });
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilityParentOutcomes_DiscreteProbabilities_DiscreteProbabilityId",
                        column: x => x.DiscreteProbabilityId,
                        principalTable: "DiscreteProbabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscreteProbabilityParentOutcomes_Outcomes_ParentOutcomeId",
                        column: x => x.ParentOutcomeId,
                        principalTable: "Outcomes",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "ValueMetrics",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { new Guid("288e0811-7ab6-5d24-b80c-9fa925b848a6"), new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Unspecified).AddTicks(1), new TimeSpan(0, 0, 0, 0, 0)), "value", new DateTimeOffset(new DateTime(2020, 1, 1, 1, 1, 1, 1, DateTimeKind.Unspecified).AddTicks(2), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CreatedById",
                table: "Assessments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ProjectId",
                table: "Assessments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_UpdatedById",
                table: "Assessments",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionQualityAssessments_AssessmentId",
                table: "DecisionQualityAssessments",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionQualityAssessments_CreatedById",
                table: "DecisionQualityAssessments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DecisionQualityAssessments_UpdatedById",
                table: "DecisionQualityAssessments",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Decisions_IssueId",
                table: "Decisions",
                column: "IssueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteProbabilities_OutcomeId",
                table: "DiscreteProbabilities",
                column: "OutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteProbabilities_UncertaintyId",
                table: "DiscreteProbabilities",
                column: "UncertaintyId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteProbabilityParentOptions_ParentOptionId",
                table: "DiscreteProbabilityParentOptions",
                column: "ParentOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteProbabilityParentOutcomes_ParentOutcomeId",
                table: "DiscreteProbabilityParentOutcomes",
                column: "ParentOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteUtilities_UtilityId",
                table: "DiscreteUtilities",
                column: "UtilityId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteUtilities_ValueMetricId",
                table: "DiscreteUtilities",
                column: "ValueMetricId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteUtilityParentOptions_ParentOptionId",
                table: "DiscreteUtilityParentOptions",
                column: "ParentOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscreteUtilityParentOutcomes_ParentOutcomeId",
                table: "DiscreteUtilityParentOutcomes",
                column: "ParentOutcomeId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_HeadId",
                table: "Edges",
                column: "HeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_ProjectId",
                table: "Edges",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_TailId",
                table: "Edges",
                column: "TailId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_CreatedById",
                table: "Issues",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ProjectId",
                table: "Issues",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_UpdatedById",
                table: "Issues",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_IssueId",
                table: "Nodes",
                column: "IssueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ProjectId",
                table: "Nodes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeStyles_NodeId",
                table: "NodeStyles",
                column: "NodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_CreatedById",
                table: "Objectives",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_ProjectId",
                table: "Objectives",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_UpdatedById",
                table: "Objectives",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Options_DecisionId",
                table: "Options",
                column: "DecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Outcomes_UncertaintyId",
                table: "Outcomes",
                column: "UncertaintyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_CreatedById",
                table: "ProjectRoles",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_ProjectId",
                table: "ProjectRoles",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_UpdatedById",
                table: "ProjectRoles",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRoles_UserId",
                table: "ProjectRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UpdatedById",
                table: "Projects",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_CreatedById",
                table: "Strategies",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_ProjectId",
                table: "Strategies",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_UpdatedById",
                table: "Strategies",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyOptions_OptionId",
                table: "StrategyOptions",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Uncertainties_IssueId",
                table: "Uncertainties",
                column: "IssueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_IssueId",
                table: "Utilities",
                column: "IssueId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DecisionQualityAssessments");

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
                name: "Assessments");

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
