using System.Net;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class ProjectsControllerTests
{
    private readonly PrismaApiFixture _fixture;
    private readonly HttpClient _client;

    public ProjectsControllerTests(PrismaApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.ApiFactory.CreateClient();
    }

    [Fact]
    public async Task GetAllProjects_ReturnsProjectsForUser()
    {
        using var scope = _fixture.UserScope();

        var response = await _client.TestClientGetAsync<List<ProjectOutgoingDto>>("projects");

        Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        Assert.Contains(response.Value, project => project.Id == _fixture.TestArgs.TestProjectId);
    }

    [Fact]
    public async Task GetProject_ReturnsProject()
    {
        using var scope = _fixture.UserScope();

        var response = await _client.TestClientGetAsync<ProjectOutgoingDto>($"projects/{_fixture.TestArgs.TestProjectId}");

        Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        Assert.Equal(_fixture.TestArgs.TestProjectId, response.Value.Id);
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

        var response = await _client.TestClientPostAsync<List<ProjectOutgoingDto>>("projects", payload);

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

        var response = await _client.TestClientPutAsync<List<ProjectOutgoingDto>>("projects", payload);

        Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        Assert.Contains(response.Value, project => project.Id == _fixture.TestArgs.TestProjectId && project.Name == updatedName);
    }

    [Fact]
    public async Task DeleteProject_RemovesProject()
    {
        using var scope = _fixture.UserScope();

        var projectId = Guid.NewGuid();
        var createPayload = new List<ProjectCreateDto>
        {
            new()
            {
                Id = projectId,
                Name = "Project To Delete",
                OpportunityStatement = "Delete test",
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

        var createResponse = await _client.TestClientPostAsync<List<ProjectOutgoingDto>>("projects", createPayload);
        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);

        var deleteResponse = await _client.TestClientDeleteAsync<string>($"projects/{projectId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }
}
