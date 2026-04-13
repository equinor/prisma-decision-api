using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class StrategiesControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public StrategiesControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task CreateStrategies_ReturnsStrategies()
    {
        using var scope = _fixture.UserScope();

        var projectId = _fixture.TestArgs.TestProjectId;
        var optionId = _fixture.TestArgs.OptionId;

        var strategyId = Guid.NewGuid();
        var createPayload = new List<StrategyIncomingDto>
        {
            new()
            {
                Id = strategyId,
                ProjectId = projectId,
                Name = "Strategy A",
                Description = "Strategy A description",
                Rationale = "Strategy A rationale",
                Icon = "icon-a",
                IconColor = "#111111",
                Options = new List<OptionIncomingDto>
                {
                    new()
                    {
                        Id = optionId,
                        DecisionId = _fixture.TestArgs.DecisionIssueId,
                        Name = "Primary Option",
                        Utility = 1.1
                    }
                }
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<StrategyOutgoingDto>>("strategies", createPayload);

        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);
        Assert.Contains(createResponse.Value, strategy => strategy.Id == strategyId);
    }

    [Fact]
    public async Task GetStrategy_ReturnsStrategy()
    {
        using var scope = _fixture.UserScope();

        var strategyId = _fixture.TestArgs.StrategyId;

        var getResponse = await Client.TestClientGetAsync<StrategyOutgoingDto>($"strategies/{strategyId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(strategyId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetStrategyWithoutAccess_ReturnsNotFound()
    {
        using var scope = _fixture.SecondaryUserScope();

        var strategyId = _fixture.TestArgs.StrategyId;

        var getResponse = await Client.TestClientGetAsync<StrategyOutgoingDto>($"strategies/{strategyId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.Response.StatusCode);
    }

    [Fact]
    public async Task GetAllStrategies_ReturnsStrategies()
    {
        using var scope = _fixture.UserScope();

        var strategyId = _fixture.TestArgs.StrategyId;

        var getAllResponse = await Client.TestClientGetAsync<List<StrategyOutgoingDto>>("strategies");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, strategy => strategy.Id == strategyId);
    }

    [Fact]
    public async Task UpdateStrategies_UpdatesStrategies()
    {
        using var scope = _fixture.UserScope();

        var strategyId = _fixture.TestArgs.StrategyId;
        var projectId = _fixture.TestArgs.TestProjectId;
        var optionId = _fixture.TestArgs.OptionId;

        var updatedName = "Strategy A Updated";
        var updatePayload = new List<StrategyIncomingDto>
        {
            new()
            {
                Id = strategyId,
                ProjectId = projectId,
                Name = updatedName,
                Description = "Strategy A description",
                Rationale = "Strategy A rationale",
                Icon = "icon-a",
                IconColor = "#111111",
                Options = new List<OptionIncomingDto>
                {
                    new()
                    {
                        Id = optionId,
                        DecisionId = _fixture.TestArgs.DecisionIssueId,
                        Name = "Primary Option",
                        Utility = 1.1
                    }
                }
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<StrategyOutgoingDto>>("strategies", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, strategy => strategy.Id == strategyId && strategy.Name == updatedName);
    }

    [Fact]
    public async Task DeleteStrategy_RemovesStrategy()
    {
        using var scope = _fixture.UserScope();

        var strategyId = _fixture.TestArgs.StrategyDeleteId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"strategies/{strategyId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteStrategies_RemovesStrategies()
    {
        using var scope = _fixture.UserScope();

        var strategyIdForBulkDelete = _fixture.TestArgs.StrategyBulkDeleteId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"strategies?ids={strategyIdForBulkDelete}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }
}
