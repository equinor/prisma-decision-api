using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class EdgesControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public EdgesControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task CreateEdges_ReturnsEdges()
    {
        using var scope = _fixture.UserScope();

        var projectId = _fixture.TestArgs.TestProjectId;
        var tailId = _fixture.TestArgs.DecisionIssueId;
        var headId = _fixture.TestArgs.UncertaintyIssueId;

        var edgeId = Guid.NewGuid();
        var createPayload = new List<EdgeIncomingDto>
        {
            new()
            {
                Id = edgeId,
                ProjectId = projectId,
                TailId = tailId,
                HeadId = headId
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<EdgeOutgoingDto>>("edges", createPayload);

        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);
        Assert.Contains(createResponse.Value, edge => edge.Id == edgeId);
    }

    [Fact]
    public async Task GetEdge_ReturnsEdge()
    {
        using var scope = _fixture.UserScope();

        var edgeId = _fixture.TestArgs.EdgeId;

        var getResponse = await Client.TestClientGetAsync<EdgeOutgoingDto>($"edges/{edgeId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(edgeId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetEdgeWithoutProjectAccess_ReturnsNotFound()
    {
        using var scope = _fixture.SecondaryUserScope();

        var edgeId = _fixture.TestArgs.EdgeId;

        var getResponse = await Client.TestClientGetAsync<EdgeOutgoingDto>($"edges/{edgeId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.Response.StatusCode);
    }

    [Fact]
    public async Task GetAllEdges_ReturnsEdges()
    {
        using var scope = _fixture.UserScope();

        var edgeId = _fixture.TestArgs.EdgeId;

        var getAllResponse = await Client.TestClientGetAsync<List<EdgeOutgoingDto>>("edges");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, edge => edge.Id == edgeId);
    }

    [Fact]
    public async Task UpdateEdges_UpdatesEdges()
    {
        using var scope = _fixture.UserScope();

        var edgeId = _fixture.TestArgs.EdgeId;
        var projectId = _fixture.TestArgs.TestProjectId;
        var tailId = _fixture.TestArgs.DecisionIssueId;
        var headId = _fixture.TestArgs.DecisionIssue2Id;

        var updatePayload = new List<EdgeIncomingDto>
        {
            new()
            {
                Id = edgeId,
                ProjectId = projectId,
                TailId = tailId,
                HeadId = headId
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<EdgeOutgoingDto>>("edges", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, edge => edge.Id == edgeId && edge.HeadId == headId);
    }

    [Fact]
    public async Task DeleteEdge_RemovesEdge()
    {
        using var scope = _fixture.UserScope();

        var edgeId = _fixture.TestArgs.EdgeDeleteId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"edges/{edgeId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteEdges_RemovesEdges()
    {
        using var scope = _fixture.UserScope();

        var edgeIdForBulkDelete = _fixture.TestArgs.EdgeBulkDeleteId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"edges?ids={edgeIdForBulkDelete}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }
}
