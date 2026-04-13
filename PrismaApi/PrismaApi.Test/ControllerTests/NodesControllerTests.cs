using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class NodesControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public NodesControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task GetNode_ReturnsNode()
    {
        using var scope = _fixture.UserScope();

        var nodeId = _fixture.TestArgs.DecisionIssueId;

        var getResponse = await Client.TestClientGetAsync<NodeOutgoingDto>($"nodes/{nodeId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(nodeId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetNodeWithoutAccess_ReturnsNotfound()
    {
        using var scope = _fixture.SecondaryUserScope();

        var nodeId = _fixture.TestArgs.DecisionIssueId;

        var getResponse = await Client.TestClientGetAsync<NodeOutgoingDto>($"nodes/{nodeId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.Response.StatusCode);
    }

    [Fact]
    public async Task GetAllNodes_ReturnsNodes()
    {
        using var scope = _fixture.UserScope();

        var nodeId = _fixture.TestArgs.DecisionIssueId;

        var getAllResponse = await Client.TestClientGetAsync<List<NodeOutgoingDto>>("nodes");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, node => node.Id == nodeId);
    }

    [Fact]
    public async Task UpdateNodes_UpdatesNodes()
    {
        using var scope = _fixture.UserScope();

        var projectId = _fixture.TestArgs.TestProjectId;
        var nodeId = _fixture.TestArgs.DecisionIssueId;

        var updatedName = "Node Updated";
        var updatePayload = new List<NodeIncomingDto>
        {
            new()
            {
                Id = nodeId,
                ProjectId = projectId,
                IssueId = nodeId,
                Name = updatedName
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<NodeOutgoingDto>>("nodes", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, node => node.Id == nodeId && node.Name == updatedName);
    }
}
