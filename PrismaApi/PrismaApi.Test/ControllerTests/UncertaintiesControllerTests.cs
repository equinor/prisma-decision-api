using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class UncertaintiesControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public UncertaintiesControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task GetUncertainty_ReturnsUncertainty()
    {
        using var scope = _fixture.UserScope();

        var uncertaintyId = _fixture.TestArgs.UncertaintyIssueId;

        var getResponse = await Client.TestClientGetAsync<UncertaintyOutgoingDto>($"uncertainties/{uncertaintyId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(uncertaintyId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetUncertaintyWithoutAccess_ReturnsNotFound()
    {
        using var scope = _fixture.SecondaryUserScope();

        var uncertaintyId = _fixture.TestArgs.UncertaintyIssueId;

        var getResponse = await Client.TestClientGetAsync<UncertaintyOutgoingDto>($"uncertainties/{uncertaintyId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.Response.StatusCode);
    }

    [Fact]
    public async Task GetAllUncertainties_ReturnsUncertainties()
    {
        using var scope = _fixture.UserScope();

        var uncertaintyId = _fixture.TestArgs.UncertaintyIssueId;

        var getAllResponse = await Client.TestClientGetAsync<List<UncertaintyOutgoingDto>>("uncertainties");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, uncertainty => uncertainty.Id == uncertaintyId);
    }

    [Fact]
    public async Task UpdateUncertainties_UpdatesUncertainties()
    {
        using var scope = _fixture.UserScope();

        var uncertaintyId = _fixture.TestArgs.UncertaintyIssueId;

        var updatePayload = new List<UncertaintyIncomingDto>
        {
            new()
            {
                Id = uncertaintyId,
                IssueId = uncertaintyId,
                IsKey = false
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<UncertaintyOutgoingDto>>("uncertainties", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, uncertainty => uncertainty.Id == uncertaintyId && !uncertainty.IsKey);
    }

    [Fact]
    public async Task RemakeProbabilityTable_ReturnsOk()
    {
        using var scope = _fixture.UserScope();

        var uncertaintyId = _fixture.TestArgs.UncertaintyIssueId;

        var remakeResponse = await Client.TestClientPostNoPayloadAsync<string>($"uncertainties/{uncertaintyId}/remake-probability-table");

        Assert.Equal(HttpStatusCode.OK, remakeResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteUncertainty_RemovesUncertainty()
    {
        using var scope = _fixture.UserScope();

        var uncertaintyId = _fixture.TestArgs.UncertaintyDeleteIssueId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"uncertainties/{uncertaintyId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteUncertainties_RemovesUncertainties()
    {
        using var scope = _fixture.UserScope();

        var uncertaintyId = _fixture.TestArgs.UncertaintyBulkDeleteIssueId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"uncertainties?ids={uncertaintyId}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }
}
