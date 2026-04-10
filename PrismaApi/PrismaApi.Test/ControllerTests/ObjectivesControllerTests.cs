using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class ObjectivesControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public ObjectivesControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task CreateObjectives_ReturnsObjectives()
    {
        using var scope = _fixture.UserScope();

        var projectId = _fixture.TestArgs.TestProjectId;
        var objectiveId = Guid.NewGuid();

        var createPayload = new List<ObjectiveIncomingDto>
        {
            new()
            {
                Id = objectiveId,
                ProjectId = projectId,
                Name = "Objective A",
                Description = "Objective A description",
                Type = ObjectiveType.Fundamental.ToString()
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<ObjectiveOutgoingDto>>("objectives", createPayload);

        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);
        Assert.Contains(createResponse.Value, objective => objective.Id == objectiveId);
    }

    [Fact]
    public async Task GetObjective_ReturnsObjective()
    {
        using var scope = _fixture.UserScope();

        var objectiveId = _fixture.TestArgs.ObjectiveId;

        var getResponse = await Client.TestClientGetAsync<ObjectiveOutgoingDto>($"objectives/{objectiveId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(objectiveId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetAllObjectives_ReturnsObjectives()
    {
        using var scope = _fixture.UserScope();

        var objectiveId = _fixture.TestArgs.ObjectiveId;

        var getAllResponse = await Client.TestClientGetAsync<List<ObjectiveOutgoingDto>>("objectives");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, objective => objective.Id == objectiveId);
    }

    [Fact]
    public async Task UpdateObjectives_UpdatesObjectives()
    {
        using var scope = _fixture.UserScope();

        var projectId = _fixture.TestArgs.TestProjectId;
        var objectiveId = _fixture.TestArgs.ObjectiveId;

        var updatedName = "Objective A - Updated";
        var updatePayload = new List<ObjectiveIncomingDto>
        {
            new()
            {
                Id = objectiveId,
                ProjectId = projectId,
                Name = updatedName,
                Description = "Objective A description",
                Type = ObjectiveType.Fundamental.ToString()
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<ObjectiveOutgoingDto>>("objectives", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, objective => objective.Id == objectiveId && objective.Name == updatedName);
    }

    [Fact]
    public async Task DeleteObjective_RemovesObjective()
    {
        using var scope = _fixture.UserScope();

        var objectiveId = _fixture.TestArgs.ObjectiveDeleteId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"objectives/{objectiveId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }

    [Fact]
    public async Task DeleteObjectives_RemovesObjectives()
    {
        using var scope = _fixture.UserScope();

        var objectiveIdForBulkDelete = _fixture.TestArgs.ObjectiveBulkDeleteId;

        var bulkDeleteResponse = await Client.TestClientDeleteAsync<string>($"objectives?ids={objectiveIdForBulkDelete}");

        Assert.Equal(HttpStatusCode.NoContent, bulkDeleteResponse.Response.StatusCode);
    }
}
