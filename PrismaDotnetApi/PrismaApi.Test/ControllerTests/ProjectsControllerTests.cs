using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Data;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class ProjectsControllerTests : IClassFixture<PrismaApiFixture>
{
    private readonly PrismaApiFixture _fixture;

    public ProjectsControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
    }
    private HttpClient Client => _fixture.ApiFactory.CreateClient();
    private TestArguments TestArgs { get; set; } = new();

    [Fact]
    public async Task GetAllProjects_ReturnsProjectsForUser()
    {
        using var scope = _fixture.UserScope();

        var response = await Client.TestClientGetAsync<List<ProjectOutgoingDto>>("projects");

        Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        Assert.Contains(response.Value, project => project.Id == _fixture.TestArgs.TestProjectId);
    }

    [Fact]
    public async Task GetProject_ReturnsProject()
    {
        using var scope = _fixture.UserScope();

        var response = await Client.TestClientGetAsync<ProjectOutgoingDto>($"projects/{_fixture.TestArgs.TestProjectId}");

        Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        Assert.Equal(_fixture.TestArgs.TestProjectId, response.Value.Id);
    }

    [Fact]
    public async Task GetProjectWithoutAccess_ReturnsNotFound()
    {
        using var scope = _fixture.SecondaryUserScope();

        var response = await Client.TestClientGetAsync<ProjectOutgoingDto>($"projects/{_fixture.TestArgs.TestProjectId}");

        Assert.Equal(HttpStatusCode.NotFound, response.Response.StatusCode);
    }

    [Fact]
    public async Task CreateProjects_CreatesProject()
    {
        using var scope = _fixture.UserScope();

        var projectId = Guid.NewGuid();
        var payload = new List<ProjectCreateDto>
        {
            new()
            {
                Id = projectId,
                Name = "Integration Project",
                OpportunityStatement = "Test project",
                Users = new List<ProjectRoleCreateDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = projectId,
                        UserId = _fixture.PrismaUser.Id!,
                        Name = _fixture.PrismaUser.Name!,
                        Role = ProjectRoleType.DecisionMaker.ToString()
                    }
                }
            }
        };

        var response = await Client.TestClientPostAsync<List<ProjectOutgoingDto>>("projects", payload);

        Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        Assert.Contains(response.Value, project => project.Id == projectId);
    }

    [Fact]
    public async Task UpdateProjects_UpdatesProject()
    {
        using var scope = _fixture.UserScope();

        var updatedName = "Updated Project Name";
        var payload = new List<ProjectIncomingDto>
        {
            new()
            {
                Id = _fixture.TestArgs.TestProjectId,
                Name = updatedName,
                OpportunityStatement = "Updated statement",
                Users = new List<ProjectRoleIncomingDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = _fixture.TestArgs.TestProjectId,
                        UserId = _fixture.PrismaUser.Id!,
                        Name = _fixture.PrismaUser.Name!,
                        Role = ProjectRoleType.DecisionMaker.ToString()
                    }
                }
            }
        };

        var response = await Client.TestClientPutAsync<List<ProjectOutgoingDto>>("projects", payload);

        Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        Assert.Contains(response.Value, project => project.Id == _fixture.TestArgs.TestProjectId && project.Name == updatedName);
    }

    [Fact]
    public async Task DeleteProject_RemovesProject()
    {
        using var scope = _fixture.UserScope();

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"projects/{TestArgs.TestProjectId.ToString()}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }
}
