using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class OptionsControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public OptionsControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task CreateOptions_ReturnsOptions()
    {
        using var scope = _fixture.UserScope();

        var optionId = Guid.NewGuid();
        var createPayload = new List<OptionIncomingDto>
        {
            new()
            {
                Id = optionId,
                DecisionId = _fixture.TestArgs.DecisionIssueId,
                Name = "Option A",
                Utility = 1.5
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<OptionOutgoingDto>>("options", createPayload);

        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);
        Assert.Contains(createResponse.Value, option => option.Id == optionId);
    }

    [Fact]
    public async Task GetOption_ReturnsOption()
    {
        using var scope = _fixture.UserScope();

        var optionId = _fixture.TestArgs.OptionId;

        var getResponse = await Client.TestClientGetAsync<OptionOutgoingDto>($"options/{optionId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(optionId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetOptionWithoutAccess_ReturnsNotFound()
    {
        using var scope = _fixture.SecondaryUserScope();

        var optionId = _fixture.TestArgs.OptionId;

        var getResponse = await Client.TestClientGetAsync<OptionOutgoingDto>($"options/{optionId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.Response.StatusCode);
    }

    [Fact]
    public async Task GetAllOptions_ReturnsOptions()
    {
        using var scope = _fixture.UserScope();

        var optionId = _fixture.TestArgs.OptionId;

        var getAllResponse = await Client.TestClientGetAsync<List<OptionOutgoingDto>>("options");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, option => option.Id == optionId);
    }

    [Fact]
    public async Task UpdateOptions_UpdatesOptions()
    {
        using var scope = _fixture.UserScope();

        var optionId = _fixture.TestArgs.OptionId;
        var decisionId = _fixture.TestArgs.DecisionIssueId;

        var updatedName = "Option A Updated";
        var updatePayload = new List<OptionIncomingDto>
        {
            new()
            {
                Id = optionId,
                DecisionId = decisionId,
                Name = updatedName,
                Utility = 4.2
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<OptionOutgoingDto>>("options", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, option => option.Id == optionId && option.Name == updatedName);
    }

    [Fact]
    public async Task DeleteOption_RemovesOption()
    {
        using var scope = _fixture.UserScope();

        var optionId = _fixture.TestArgs.OptionDeleteId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"options/{optionId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteOptions_RemovesOptions()
    {
        using var scope = _fixture.UserScope();

        var optionIdForBulkDelete = _fixture.TestArgs.OptionBulkDeleteId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"options?ids={optionIdForBulkDelete}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }
}
