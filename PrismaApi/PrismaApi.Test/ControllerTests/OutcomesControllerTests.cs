using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class OutcomesControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public OutcomesControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task CreateOutcomes_ReturnsOutcomes()
    {
        using var scope = _fixture.UserScope();

        var outcomeId = Guid.NewGuid();
        var createPayload = new List<OutcomeIncomingDto>
        {
            new()
            {
                Id = outcomeId,
                UncertaintyId = _fixture.TestArgs.UncertaintyIssueId,
                Name = "Outcome A",
                Utility = 5.5
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<OutcomeOutgoingDto>>("outcomes", createPayload);

        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);
        Assert.Contains(createResponse.Value, outcome => outcome.Id == outcomeId);
    }

    [Fact]
    public async Task GetOutcome_ReturnsOutcome()
    {
        using var scope = _fixture.UserScope();

        var outcomeId = _fixture.TestArgs.OutcomeId;

        var getResponse = await Client.TestClientGetAsync<OutcomeOutgoingDto>($"outcomes/{outcomeId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(outcomeId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetOutcomeWithoutAccess_ReturnsNotFound()
    {
        using var scope = _fixture.SecondaryUserScope();

        var outcomeId = _fixture.TestArgs.OutcomeId;

        var getResponse = await Client.TestClientGetAsync<OutcomeOutgoingDto>($"outcomes/{outcomeId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.Response.StatusCode);
    }

    [Fact]
    public async Task GetAllOutcomes_ReturnsOutcomes()
    {
        using var scope = _fixture.UserScope();

        var outcomeId = _fixture.TestArgs.OutcomeId;

        var getAllResponse = await Client.TestClientGetAsync<List<OutcomeOutgoingDto>>("outcomes");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, outcome => outcome.Id == outcomeId);
    }

    [Fact]
    public async Task UpdateOutcomes_UpdatesOutcomes()
    {
        using var scope = _fixture.UserScope();

        var outcomeId = _fixture.TestArgs.OutcomeId;
        var uncertaintyId = _fixture.TestArgs.UncertaintyIssueId;

        var updatedName = "Outcome A Updated";
        var updatePayload = new List<OutcomeIncomingDto>
        {
            new()
            {
                Id = outcomeId,
                UncertaintyId = uncertaintyId,
                Name = updatedName,
                Utility = 7.3
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<OutcomeOutgoingDto>>("outcomes", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, outcome => outcome.Id == outcomeId && outcome.Name == updatedName);
    }

    [Fact]
    public async Task DeleteOutcome_RemovesOutcome()
    {
        using var scope = _fixture.UserScope();

        var outcomeId = _fixture.TestArgs.OutcomeDeleteId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"outcomes/{outcomeId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteOutcomes_RemovesOutcomes()
    {
        using var scope = _fixture.UserScope();

        var outcomeIdForBulkDelete = _fixture.TestArgs.OutcomeBulkDeleteId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"outcomes?ids={outcomeIdForBulkDelete}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }
}
