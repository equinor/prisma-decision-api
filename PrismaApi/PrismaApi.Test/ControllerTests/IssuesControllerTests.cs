using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class IssuesControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public IssuesControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task CreateIssues_ReturnsIssues()
    {
        using var scope = _fixture.UserScope();

        var projectId = _fixture.TestArgs.TestProjectId;
        var issueId = Guid.NewGuid();

        var createPayload = new List<IssueIncomingDto>
        {
            BuildDecisionIssue(issueId, projectId, 0)
        };

        var createResponse = await Client.TestClientPostAsync<List<IssueOutgoingDto>>("issues", createPayload);

        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);
        Assert.Contains(createResponse.Value, issue => issue.Id == issueId);
    }

    [Fact]
    public async Task GetIssue_ReturnsIssue()
    {
        using var scope = _fixture.UserScope();

        var issueId = _fixture.TestArgs.DecisionIssueId;

        var getResponse = await Client.TestClientGetAsync<IssueOutgoingDto>($"issues/{issueId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(issueId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetAllIssues_ReturnsIssues()
    {
        using var scope = _fixture.UserScope();

        var issueId = _fixture.TestArgs.DecisionIssueId;

        var getAllResponse = await Client.TestClientGetAsync<List<IssueOutgoingDto>>("issues");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, issue => issue.Id == issueId);
    }

    [Fact]
    public async Task UpdateIssues_UpdatesIssue()
    {
        using var scope = _fixture.UserScope();

        var projectId = _fixture.TestArgs.TestProjectId;
        var issueId = _fixture.TestArgs.DecisionIssueId;

        var updatedName = "Issue Updated";
        var updatePayload = new List<IssueIncomingDto>
        {
            BuildDecisionIssue(issueId, projectId, 2, updatedName)
        };

        var updateResponse = await Client.TestClientPutAsync<List<IssueOutgoingDto>>("issues", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, issue => issue.Id == issueId && issue.Name == updatedName);
    }

    [Fact]
    public async Task DeleteIssue_RemovesIssue()
    {
        using var scope = _fixture.UserScope();

        var issueId = _fixture.TestArgs.IssueDeleteId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"issues/{issueId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteIssues_RemovesIssues()
    {
        using var scope = _fixture.UserScope();

        var issueIdForBulkDelete = _fixture.TestArgs.IssueBulkDeleteId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"issues?ids={issueIdForBulkDelete}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }

    private static IssueIncomingDto BuildDecisionIssue(Guid issueId, Guid projectId, int order, string? nameOverride = null)
    {
        return new IssueIncomingDto
        {
            Id = issueId,
            ProjectId = projectId,
            Name = nameOverride ?? "Decision Issue",
            Description = "Decision issue description",
            Order = order,
            Type = IssueType.Decision.ToString(),
            Boundary = Boundary.On.ToString(),
            Node = new NodeIncomingDto
            {
                Id = issueId,
                ProjectId = projectId,
                IssueId = issueId,
                Name = "Decision Node"
            },
            Decision = new DecisionIncomingDto
            {
                Id = issueId,
                IssueId = issueId,
                Type = DecisionHierarchy.Focus.ToString()
            }
        };
    }
}
