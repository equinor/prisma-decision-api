using Microsoft.Extensions.DependencyInjection;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.Data;

public class TestModelBuilder
{
    internal static async Task<TestArguments> BuildFreshTestDataAsync(PrismaApiFixture fixture)
    {
        using var scope = fixture.ApiFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var anyDataFound = db.Projects.Any();
        var args = new TestArguments();

        var primaryUser = new Domain.Entities.User
        {
            Id = fixture.PrismaUser.Id!,
            Name = fixture.PrismaUser.Name!
        };
        var secondaryUser = new Domain.Entities.User
        {
            Id = fixture.SecundaryUser.Id!,
            Name = fixture.SecundaryUser.Name!
        };
        if (!anyDataFound)
        {
            db.Users.AddRange(primaryUser, secondaryUser);
        }

        var primaryProject = new Project
        {
            Id = args.TestProjectId,
            Name = "Test Project",
            CreatedById = primaryUser.Id,
            UpdatedById = primaryUser.Id,
        };
        var secondaryProject = new Project
        {
            Id = args.SecondaryProjectId,
            Name = "Test Project - Larger",
            CreatedById = secondaryUser.Id,
            UpdatedById = secondaryUser.Id,
        };

        db.Projects.AddRange(primaryProject, secondaryProject);

        db.ProjectRoles.AddRange(
            new ProjectRole
            {
                Id = Guid.NewGuid(),
                ProjectId = primaryProject.Id,
                UserId = primaryUser.Id,
                Role = ProjectRoleType.DecisionMaker.ToString(),
                CreatedById = primaryUser.Id,
                UpdatedById = primaryUser.Id
            },
            new ProjectRole
            {
                Id = Guid.NewGuid(),
                ProjectId = secondaryProject.Id,
                UserId = secondaryUser.Id,
                Role = ProjectRoleType.Member.ToString(),
                CreatedById = secondaryUser.Id,
                UpdatedById = secondaryUser.Id
            });

        static Issue BuildIssue(Guid issueId, Guid projectId, string name, string type, string boundary, int order, string userId) =>
            new()
            {
                Id = issueId,
                ProjectId = projectId,
                Type = type,
                Boundary = boundary,
                Name = name,
                Description = $"{name} description",
                Order = order,
                CreatedById = userId,
                UpdatedById = userId
            };

        static Node BuildNode(Guid issueId, Guid projectId, string name) =>
            new()
            {
                Id = issueId,
                ProjectId = projectId,
                IssueId = issueId,
                Name = name
            };

        static NodeStyle BuildNodeStyle(Guid nodeId, double xPosition, double yPosition) =>
            new()
            {
                Id = nodeId,
                NodeId = nodeId,
                XPosition = xPosition,
                YPosition = yPosition
            };

        var primaryDecision1Id = Guid.NewGuid();
        var primaryUncertainty1Id = Guid.NewGuid();
        var primaryDecision2Id = Guid.NewGuid();
        var primaryUncertainty2Id = Guid.NewGuid();

        var primaryDecision1Issue = BuildIssue(primaryDecision1Id, primaryProject.Id, "Primary Decision 1", IssueType.Decision.ToString(), Boundary.On.ToString(), 0, primaryUser.Id);
        var primaryUncertainty1Issue = BuildIssue(primaryUncertainty1Id, primaryProject.Id, "Primary Uncertainty 1", IssueType.Uncertainty.ToString(), Boundary.In.ToString(), 1, primaryUser.Id);
        var primaryDecision2Issue = BuildIssue(primaryDecision2Id, primaryProject.Id, "Primary Decision 2", IssueType.Decision.ToString(), Boundary.On.ToString(), 2, primaryUser.Id);
        var primaryUncertainty2Issue = BuildIssue(primaryUncertainty2Id, primaryProject.Id, "Primary Uncertainty 2", IssueType.Uncertainty.ToString(), Boundary.In.ToString(), 3, primaryUser.Id);

        db.Issues.AddRange(primaryDecision1Issue, primaryUncertainty1Issue, primaryDecision2Issue, primaryUncertainty2Issue);
        db.Nodes.AddRange(
            BuildNode(primaryDecision1Id, primaryProject.Id, primaryDecision1Issue.Name),
            BuildNode(primaryUncertainty1Id, primaryProject.Id, primaryUncertainty1Issue.Name),
            BuildNode(primaryDecision2Id, primaryProject.Id, primaryDecision2Issue.Name),
            BuildNode(primaryUncertainty2Id, primaryProject.Id, primaryUncertainty2Issue.Name));
        db.NodeStyles.AddRange(
            BuildNodeStyle(primaryDecision1Id, 0, 0),
            BuildNodeStyle(primaryUncertainty1Id, 150, 0),
            BuildNodeStyle(primaryDecision2Id, 300, 0),
            BuildNodeStyle(primaryUncertainty2Id, 450, 0));
        db.Decisions.AddRange(
            new Decision { Id = primaryDecision1Id, IssueId = primaryDecision1Id, Type = IssueType.Decision.ToString() },
            new Decision { Id = primaryDecision2Id, IssueId = primaryDecision2Id, Type = IssueType.Decision.ToString() });
        db.Uncertainties.AddRange(
            new Uncertainty { Id = primaryUncertainty1Id, IssueId = primaryUncertainty1Id, IsKey = true },
            new Uncertainty { Id = primaryUncertainty2Id, IssueId = primaryUncertainty2Id, IsKey = true });
        db.Edges.AddRange(
            new Edge { Id = Guid.NewGuid(), ProjectId = primaryProject.Id, TailId = primaryDecision1Id, HeadId = primaryUncertainty1Id },
            new Edge { Id = Guid.NewGuid(), ProjectId = primaryProject.Id, TailId = primaryUncertainty1Id, HeadId = primaryDecision2Id },
            new Edge { Id = Guid.NewGuid(), ProjectId = primaryProject.Id, TailId = primaryDecision2Id, HeadId = primaryUncertainty2Id });

        var secondaryDecision1Id = Guid.NewGuid();
        var secondaryUncertainty1Id = Guid.NewGuid();
        var secondaryDecision2Id = Guid.NewGuid();
        var secondaryUncertainty2Id = Guid.NewGuid();
        var secondaryDecision3Id = Guid.NewGuid();
        var secondaryUncertainty3Id = Guid.NewGuid();

        var secondaryDecision1Issue = BuildIssue(secondaryDecision1Id, secondaryProject.Id, "Secondary Decision 1", IssueType.Decision.ToString(), Boundary.On.ToString(), 0, secondaryUser.Id);
        var secondaryUncertainty1Issue = BuildIssue(secondaryUncertainty1Id, secondaryProject.Id, "Secondary Uncertainty 1", IssueType.Uncertainty.ToString(), Boundary.In.ToString(), 1, secondaryUser.Id);
        var secondaryDecision2Issue = BuildIssue(secondaryDecision2Id, secondaryProject.Id, "Secondary Decision 2", IssueType.Decision.ToString(), Boundary.On.ToString(), 2, secondaryUser.Id);
        var secondaryUncertainty2Issue = BuildIssue(secondaryUncertainty2Id, secondaryProject.Id, "Secondary Uncertainty 2", IssueType.Uncertainty.ToString(), Boundary.In.ToString(), 3, secondaryUser.Id);
        var secondaryDecision3Issue = BuildIssue(secondaryDecision3Id, secondaryProject.Id, "Secondary Decision 3", IssueType.Decision.ToString(), Boundary.On.ToString(), 4, secondaryUser.Id);
        var secondaryUncertainty3Issue = BuildIssue(secondaryUncertainty3Id, secondaryProject.Id, "Secondary Uncertainty 3", IssueType.Uncertainty.ToString(), Boundary.In.ToString(), 5, secondaryUser.Id);

        db.Issues.AddRange(
            secondaryDecision1Issue,
            secondaryUncertainty1Issue,
            secondaryDecision2Issue,
            secondaryUncertainty2Issue,
            secondaryDecision3Issue,
            secondaryUncertainty3Issue);
        db.Nodes.AddRange(
            BuildNode(secondaryDecision1Id, secondaryProject.Id, secondaryDecision1Issue.Name),
            BuildNode(secondaryUncertainty1Id, secondaryProject.Id, secondaryUncertainty1Issue.Name),
            BuildNode(secondaryDecision2Id, secondaryProject.Id, secondaryDecision2Issue.Name),
            BuildNode(secondaryUncertainty2Id, secondaryProject.Id, secondaryUncertainty2Issue.Name),
            BuildNode(secondaryDecision3Id, secondaryProject.Id, secondaryDecision3Issue.Name),
            BuildNode(secondaryUncertainty3Id, secondaryProject.Id, secondaryUncertainty3Issue.Name));
        db.NodeStyles.AddRange(
            BuildNodeStyle(secondaryDecision1Id, 0, 120),
            BuildNodeStyle(secondaryUncertainty1Id, 150, 120),
            BuildNodeStyle(secondaryDecision2Id, 300, 120),
            BuildNodeStyle(secondaryUncertainty2Id, 450, 120),
            BuildNodeStyle(secondaryDecision3Id, 600, 120),
            BuildNodeStyle(secondaryUncertainty3Id, 750, 120));
        db.Decisions.AddRange(
            new Decision { Id = secondaryDecision1Id, IssueId = secondaryDecision1Id, Type = IssueType.Decision.ToString() },
            new Decision { Id = secondaryDecision2Id, IssueId = secondaryDecision2Id, Type = IssueType.Decision.ToString() },
            new Decision { Id = secondaryDecision3Id, IssueId = secondaryDecision3Id, Type = IssueType.Decision.ToString() });
        db.Uncertainties.AddRange(
            new Uncertainty { Id = secondaryUncertainty1Id, IssueId = secondaryUncertainty1Id, IsKey = true },
            new Uncertainty { Id = secondaryUncertainty2Id, IssueId = secondaryUncertainty2Id, IsKey = true },
            new Uncertainty { Id = secondaryUncertainty3Id, IssueId = secondaryUncertainty3Id, IsKey = true });
        db.Edges.AddRange(
            new Edge { Id = Guid.NewGuid(), ProjectId = secondaryProject.Id, TailId = secondaryDecision1Id, HeadId = secondaryUncertainty1Id },
            new Edge { Id = Guid.NewGuid(), ProjectId = secondaryProject.Id, TailId = secondaryUncertainty1Id, HeadId = secondaryDecision2Id },
            new Edge { Id = Guid.NewGuid(), ProjectId = secondaryProject.Id, TailId = secondaryDecision2Id, HeadId = secondaryUncertainty2Id },
            new Edge { Id = Guid.NewGuid(), ProjectId = secondaryProject.Id, TailId = secondaryUncertainty2Id, HeadId = secondaryDecision3Id },
            new Edge { Id = Guid.NewGuid(), ProjectId = secondaryProject.Id, TailId = secondaryDecision3Id, HeadId = secondaryUncertainty3Id });

        await db.SaveChangesAsync();
        return args;
    }
}

public class TestArguments
{

    public Guid TestProjectId { get; set; } = Guid.NewGuid();
    public Guid SecondaryProjectId { get; set; } = Guid.NewGuid();
    public string SecondaryUserId { get; set; } = string.Empty;

    public static int GenerateUniqueId()
    {
        var ticks = DateTime.UtcNow.Ticks;
        var randomNumber = new Random().Next();
        return (int)((ticks / TimeSpan.TicksPerSecond) ^ randomNumber);
    }
}
