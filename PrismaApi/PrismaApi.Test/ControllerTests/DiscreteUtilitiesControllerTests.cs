using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class DiscreteUtilitiesControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public DiscreteUtilitiesControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task CreateDiscreteUtilities_ReturnsUtilities()
    {
        using var scope = _fixture.UserScope();

        var utilityId = _fixture.TestArgs.UtilityIssueId;
        var discreteUtilityId = Guid.NewGuid();
        var createPayload = new List<DiscreteUtilityDto>
        {
            new()
            {
                Id = discreteUtilityId,
                UtilityId = utilityId,
                ValueMetricId = DomainConstants.DefaultValueMetricId,
                UtilityValue = 3.14
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<DiscreteUtilityDto>>("discrete_utilities", createPayload);

        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);
        Assert.Contains(createResponse.Value, utility => utility.Id == discreteUtilityId);
    }

    [Fact]
    public async Task GetDiscreteUtility_ReturnsUtility()
    {
        using var scope = _fixture.UserScope();

        var discreteUtilityId = _fixture.TestArgs.DiscreteUtilityId;

        var getResponse = await Client.TestClientGetAsync<DiscreteUtilityDto>($"discrete_utilities/{discreteUtilityId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(discreteUtilityId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetAllDiscreteUtilities_ReturnsUtilities()
    {
        using var scope = _fixture.UserScope();

        var discreteUtilityId = _fixture.TestArgs.DiscreteUtilityId;

        var getAllResponse = await Client.TestClientGetAsync<List<DiscreteUtilityDto>>("discrete_utilities");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, utility => utility.Id == discreteUtilityId);
    }

    [Fact]
    public async Task UpdateDiscreteUtilities_UpdatesUtilities()
    {
        using var scope = _fixture.UserScope();

        var utilityId = _fixture.TestArgs.UtilityIssueId;
        var discreteUtilityId = _fixture.TestArgs.DiscreteUtilityId;

        var updatePayload = new List<DiscreteUtilityDto>
        {
            new()
            {
                Id = discreteUtilityId,
                UtilityId = utilityId,
                ValueMetricId = DomainConstants.DefaultValueMetricId,
                UtilityValue = 4.5
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<DiscreteUtilityDto>>("discrete_utilities", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, utility => utility.Id == discreteUtilityId && utility.UtilityValue == 4.5);
    }

    [Fact]
    public async Task DeleteDiscreteUtility_RemovesUtility()
    {
        using var scope = _fixture.UserScope();

        var discreteUtilityId = _fixture.TestArgs.DiscreteUtilityDeleteId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"discrete_utilities/{discreteUtilityId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteDiscreteUtilities_RemovesUtilities()
    {
        using var scope = _fixture.UserScope();

        var discreteUtilityIdForBulkDelete = _fixture.TestArgs.DiscreteUtilityBulkDeleteId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"discrete_utilities?ids={discreteUtilityIdForBulkDelete}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }
}
