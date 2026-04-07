using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class DecisionsControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public DecisionsControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task GetDecision_ReturnsDecision()
    {
        using var scope = _fixture.UserScope();

        var decisionId = _fixture.TestArgs.DecisionIssueId;

        var getResponse = await Client.TestClientGetAsync<DecisionOutgoingDto>($"decisions/{decisionId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(decisionId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetAllDecisions_ReturnsDecisions()
    {
        using var scope = _fixture.UserScope();

        var decisionId = _fixture.TestArgs.DecisionIssueId;

        var getAllResponse = await Client.TestClientGetAsync<List<DecisionOutgoingDto>>("decisions");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, decision => decision.Id == decisionId);
    }

    [Fact]
    public async Task UpdateDecisions_UpdatesDecisions()
    {
        using var scope = _fixture.UserScope();

        var decisionId = _fixture.TestArgs.DecisionIssueId;

        var updatePayload = new List<DecisionIncomingDto>
        {
            new()
            {
                Id = decisionId,
                IssueId = decisionId,
                Type = DecisionHierarchy.Tactical.ToString()
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<DecisionOutgoingDto>>("decisions", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, decision => decision.Id == decisionId && decision.Type == DecisionHierarchy.Tactical.ToString());
    }

    [Fact]
    public async Task DeleteDecision_RemovesDecision()
    {
        using var scope = _fixture.UserScope();

        var decisionId = _fixture.TestArgs.DecisionDeleteIssueId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"decisions/{decisionId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteDecisions_RemovesDecisions()
    {
        using var scope = _fixture.UserScope();

        var decisionId = _fixture.TestArgs.DecisionBulkDeleteIssueId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"decisions?ids={decisionId}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }
}
