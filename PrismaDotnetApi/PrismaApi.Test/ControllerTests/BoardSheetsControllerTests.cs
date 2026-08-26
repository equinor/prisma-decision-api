using System.Net;
using PrismaApi.Domain.Dtos;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class BoardSheetsControllerTests : PrismaApiControllerTestBase
{
    private readonly PrismaApiFixture _fixture;

    public BoardSheetsControllerTests(PrismaApiFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task CreateBoardSheets_ReturnsBoardSheets()
    {
        using var scope = _fixture.UserScope();

        var projectId = _fixture.TestArgs.TestProjectId;
        var boardSheetId = Guid.NewGuid();

        var createPayload = new List<BoardSheetIncomingDto>
        {
            new()
            {
                Id = boardSheetId,
                ProjectId = projectId,
                Name = "New Board Sheet"
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<BoardSheetOutgoingDto>>("board_sheets", createPayload);

        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);
        Assert.Contains(createResponse.Value, s => s.Id == boardSheetId);
    }

    [Fact]
    public async Task GetBoardSheet_ReturnsBoardSheet()
    {
        using var scope = _fixture.UserScope();

        var boardSheetId = _fixture.TestArgs.BoardSheetId;

        var getResponse = await Client.TestClientGetAsync<BoardSheetOutgoingDto>($"board_sheets/{boardSheetId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.Response.StatusCode);
        Assert.Equal(boardSheetId, getResponse.Value.Id);
    }

    [Fact]
    public async Task GetBoardSheetWithoutProjectAccess_ReturnsNotFound()
    {
        using var scope = _fixture.SecondaryUserScope();

        var boardSheetId = _fixture.TestArgs.BoardSheetId;

        var getResponse = await Client.TestClientGetAsync<BoardSheetOutgoingDto>($"board_sheets/{boardSheetId}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.Response.StatusCode);
    }

    [Fact]
    public async Task GetAllBoardSheets_ReturnsBoardSheets()
    {
        using var scope = _fixture.UserScope();

        var boardSheetId = _fixture.TestArgs.BoardSheetId;

        var getAllResponse = await Client.TestClientGetAsync<List<BoardSheetOutgoingDto>>("board_sheets");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.Response.StatusCode);
        Assert.Contains(getAllResponse.Value, s => s.Id == boardSheetId);
    }

    [Fact]
    public async Task UpdateBoardSheets_UpdatesBoardSheets()
    {
        using var scope = _fixture.UserScope();

        var boardSheetId = _fixture.TestArgs.BoardSheetId;
        var projectId = _fixture.TestArgs.TestProjectId;
        var updatedName = "Updated Board Sheet";

        var updatePayload = new List<BoardSheetIncomingDto>
        {
            new()
            {
                Id = boardSheetId,
                ProjectId = projectId,
                Name = updatedName
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<BoardSheetOutgoingDto>>("board_sheets", updatePayload);

        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);
        Assert.Contains(updateResponse.Value, s => s.Id == boardSheetId && s.Name == updatedName);
    }

    [Fact]
    public async Task DeleteBoardSheet_RemovesBoardSheet()
    {
        using var scope = _fixture.UserScope();

        var boardSheetId = _fixture.TestArgs.BoardSheetDeleteId;

        var deleteResponse = await Client.TestClientDeleteAsync<string>($"board_sheets/{boardSheetId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.Response.StatusCode);
    }
}
