using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Test.Configuration.Extensions;
using PrismaApi.Test.Fixture;

namespace PrismaApi.Test.ControllerTests;

[Collection(nameof(PrismaCollection))]
public class RestrictionTableDuplicationTests : PrismaApiControllerTestBase
{
    private readonly PrismaApiFixture _fixture;

    public RestrictionTableDuplicationTests(PrismaApiFixture fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    private HttpClient Client => _fixture.ApiFactory.CreateClient();

    [Fact]
    public async Task ImportProject_PreservesProbabilityTableOrderAndValuesWhenCreatedAtMissing()
    {
        using var scope = _fixture.UserScope();

        var projectId = Guid.NewGuid();
        var decisionIssueId = Guid.NewGuid();
        var uncertaintyIssueId = Guid.NewGuid();
        var decisionNodeId = Guid.NewGuid();
        var uncertaintyNodeId = Guid.NewGuid();
        var option1Id = Guid.NewGuid();
        var option2Id = Guid.NewGuid();
        var outcome2Id = Guid.NewGuid();
        var outcome3Id = Guid.NewGuid();
        var outcome1Id = Guid.NewGuid();
        var edgeId = Guid.NewGuid();
        var restrictionTableId = Guid.NewGuid();

        var probabilityByNamePair = new Dictionary<(string OptionName, string OutcomeName), double>
        {
            [("opt1", "out2")] = 0.2,
            [("opt1", "out3")] = 0.6,
            [("opt1", "out1")] = 0.2,
            [("opt2", "out2")] = 0.1,
            [("opt2", "out3")] = 0.3,
            [("opt2", "out1")] = 0.6,
        };

        var restrictionValueByNamePair = new Dictionary<(string OptionName, string OutcomeName), double>
        {
            [("opt1", "out2")] = 1,
            [("opt1", "out3")] = 0,
            [("opt1", "out1")] = 1,
            [("opt2", "out2")] = 1,
            [("opt2", "out3")] = 1,
            [("opt2", "out1")] = 1,
        };

        var importPayload = new List<ProjectImportDto>
        {
            new()
            {
                Projects = new ProjectIncomingDto
                {
                    Id = projectId,
                    Name = "Imported Probability Table Project",
                    OpportunityStatement = "Import probability table ordering test",
                    Users = new List<ProjectRoleIncomingDto>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = projectId,
                            UserId = _fixture.PrismaUser.Id!,
                            Name = _fixture.PrismaUser.Name!,
                            Role = ProjectRoleType.Facilitator.ToString()
                        }
                    }
                },
                Issues = new List<IssueIncomingDto>
                {
                    new()
                    {
                        Id = decisionIssueId,
                        ProjectId = projectId,
                        Name = "ds1",
                        Type = IssueType.Decision.ToString(),
                        Boundary = Boundary.On.ToString(),
                        Order = 0,
                        Node = new NodeIncomingDto
                        {
                            Id = decisionNodeId,
                            ProjectId = projectId,
                            IssueId = decisionIssueId,
                            Name = "ds1",
                            NodeStyle = new NodeStyleIncomingDto { Id = Guid.NewGuid(), NodeId = decisionNodeId, XPosition = 0, YPosition = 0 }
                        },
                        Decision = new DecisionIncomingDto
                        {
                            Id = decisionIssueId,
                            IssueId = decisionIssueId,
                            ProjectId = projectId,
                            Type = DecisionHierarchy.Focus.ToString(),
                            Options = new List<OptionIncomingDto>
                            {
                                new() { Id = option1Id, ProjectId = projectId, DecisionId = decisionIssueId, Name = "opt1" },
                                new() { Id = option2Id, ProjectId = projectId, DecisionId = decisionIssueId, Name = "opt2" }
                            }
                        }
                    },
                    new()
                    {
                        Id = uncertaintyIssueId,
                        ProjectId = projectId,
                        Name = "ut1",
                        Type = IssueType.Uncertainty.ToString(),
                        Boundary = Boundary.In.ToString(),
                        Order = 1,
                        Node = new NodeIncomingDto
                        {
                            Id = uncertaintyNodeId,
                            ProjectId = projectId,
                            IssueId = uncertaintyIssueId,
                            Name = "ut1",
                            NodeStyle = new NodeStyleIncomingDto { Id = Guid.NewGuid(), NodeId = uncertaintyNodeId, XPosition = 100, YPosition = 0 }
                        },
                        Uncertainty = new UncertaintyIncomingDto
                        {
                            Id = uncertaintyIssueId,
                            IssueId = uncertaintyIssueId,
                            ProjectId = projectId,
                            IsKey = true,
                            Outcomes = new List<OutcomeIncomingDto>
                            {
                                new() { Id = outcome2Id, ProjectId = projectId, UncertaintyId = uncertaintyIssueId, Name = "out2" },
                                new() { Id = outcome3Id, ProjectId = projectId, UncertaintyId = uncertaintyIssueId, Name = "out3" },
                                new() { Id = outcome1Id, ProjectId = projectId, UncertaintyId = uncertaintyIssueId, Name = "out1" }
                            }
                        }
                    }
                },
                Edges = new List<EdgeIncomingDto>
                {
                    new() { Id = edgeId, ProjectId = projectId, TailId = decisionNodeId, HeadId = uncertaintyNodeId }
                },
                DiscreteProbabilities = new List<DiscreteProbabilityDto>
                {
                    new() { Id = Guid.NewGuid(), ProjectId = projectId, UncertaintyId = uncertaintyIssueId, OutcomeId = outcome2Id, ParentOptionIds = [option1Id], Probability = probabilityByNamePair[("opt1", "out2")] },
                    new() { Id = Guid.NewGuid(), ProjectId = projectId, UncertaintyId = uncertaintyIssueId, OutcomeId = outcome3Id, ParentOptionIds = [option1Id], Probability = probabilityByNamePair[("opt1", "out3")] },
                    new() { Id = Guid.NewGuid(), ProjectId = projectId, UncertaintyId = uncertaintyIssueId, OutcomeId = outcome1Id, ParentOptionIds = [option1Id], Probability = probabilityByNamePair[("opt1", "out1")] },
                    new() { Id = Guid.NewGuid(), ProjectId = projectId, UncertaintyId = uncertaintyIssueId, OutcomeId = outcome2Id, ParentOptionIds = [option2Id], Probability = probabilityByNamePair[("opt2", "out2")] },
                    new() { Id = Guid.NewGuid(), ProjectId = projectId, UncertaintyId = uncertaintyIssueId, OutcomeId = outcome3Id, ParentOptionIds = [option2Id], Probability = probabilityByNamePair[("opt2", "out3")] },
                    new() { Id = Guid.NewGuid(), ProjectId = projectId, UncertaintyId = uncertaintyIssueId, OutcomeId = outcome1Id, ParentOptionIds = [option2Id], Probability = probabilityByNamePair[("opt2", "out1")] }
                },
                RestrictionTables = new List<RestrictionTableIncomingDto>
                {
                    new()
                    {
                        Id = restrictionTableId,
                        ProjectId = projectId,
                        EdgeId = edgeId,
                        Name = "ds1 to ut1 Restriction Table",
                        RestrictionEntries = new List<RestrictionEntryIncomingDto>
                        {
                            new() { Id = Guid.NewGuid(), ProjectId = projectId, RestrictionTableId = restrictionTableId, RestrictionValue = restrictionValueByNamePair[("opt1", "out2")], ParentStateId = option1Id, IsParentUncertainty = false, ChildStateId = outcome2Id, IsChildUncertainty = true },
                            new() { Id = Guid.NewGuid(), ProjectId = projectId, RestrictionTableId = restrictionTableId, RestrictionValue = restrictionValueByNamePair[("opt1", "out3")], ParentStateId = option1Id, IsParentUncertainty = false, ChildStateId = outcome3Id, IsChildUncertainty = true },
                            new() { Id = Guid.NewGuid(), ProjectId = projectId, RestrictionTableId = restrictionTableId, RestrictionValue = restrictionValueByNamePair[("opt1", "out1")], ParentStateId = option1Id, IsParentUncertainty = false, ChildStateId = outcome1Id, IsChildUncertainty = true },
                            new() { Id = Guid.NewGuid(), ProjectId = projectId, RestrictionTableId = restrictionTableId, RestrictionValue = restrictionValueByNamePair[("opt2", "out2")], ParentStateId = option2Id, IsParentUncertainty = false, ChildStateId = outcome2Id, IsChildUncertainty = true },
                            new() { Id = Guid.NewGuid(), ProjectId = projectId, RestrictionTableId = restrictionTableId, RestrictionValue = restrictionValueByNamePair[("opt2", "out3")], ParentStateId = option2Id, IsParentUncertainty = false, ChildStateId = outcome3Id, IsChildUncertainty = true },
                            new() { Id = Guid.NewGuid(), ProjectId = projectId, RestrictionTableId = restrictionTableId, RestrictionValue = restrictionValueByNamePair[("opt2", "out1")], ParentStateId = option2Id, IsParentUncertainty = false, ChildStateId = outcome1Id, IsChildUncertainty = true }
                        }
                    }
                }
            }
        };

        var importResponse = await Client.TestClientPostAsync<List<ProjectOutgoingDto>>("projects/import", importPayload);
        Assert.Equal(HttpStatusCode.OK, importResponse.Response.StatusCode);
        var importedProject = Assert.Single(importResponse.Value);

        var importedOptions = (await Client.TestClientGetAsync<List<OptionOutgoingDto>>("options")).Value
            .Where(option => option.ProjectId == importedProject.Id)
            .OrderBy(option => option.CreatedAt)
            .ToList();
        var importedOutcomes = (await Client.TestClientGetAsync<List<OutcomeOutgoingDto>>("outcomes")).Value
            .Where(outcome => outcome.ProjectId == importedProject.Id)
            .OrderBy(outcome => outcome.CreatedAt)
            .ToList();

        Assert.Equal(["opt1", "opt2"], importedOptions.Select(option => option.Name).ToList());
        Assert.Equal(["out2", "out3", "out1"], importedOutcomes.Select(outcome => outcome.Name).ToList());

        var probabilities = (await Client.TestClientGetAsync<List<DiscreteProbabilityDto>>("discrete_probabilities")).Value
            .Where(probability => probability.ProjectId == importedProject.Id)
            .ToList();

        foreach (var probability in probabilities)
        {
            var optionName = importedOptions.Single(option => probability.ParentOptionIds.Contains(option.Id)).Name;
            var outcomeName = importedOutcomes.Single(outcome => outcome.Id == probability.OutcomeId).Name;
            Assert.Equal(probabilityByNamePair[(optionName, outcomeName)], probability.Probability);
        }

        var restrictionTables = (await Client.TestClientGetAsync<List<RestrictionTableOutgoingDto>>("restriction_tables")).Value
            .Where(restrictionTable => restrictionTable.ProjectId == importedProject.Id)
            .ToList();
        var importedRestrictionTable = Assert.Single(restrictionTables);

        foreach (var restrictionEntry in importedRestrictionTable.RestrictionEntries)
        {
            var optionName = importedOptions.Single(option => option.Id == restrictionEntry.ParentStateId).Name;
            var outcomeName = importedOutcomes.Single(outcome => outcome.Id == restrictionEntry.ChildStateId).Name;
            Assert.Equal(restrictionValueByNamePair[(optionName, outcomeName)], restrictionEntry.RestrictionValue);
        }
    }

    [Fact]
    public async Task DuplicateProject_PreservesToggledOffRestrictionEntries()
    {
        using var scope = _fixture.UserScope();

        // the parent decision must be a "Focus" decision, or the rebuild step will delete the restriction table as out-of-scope
        using (var setupScope = _fixture.ApiFactory.Services.CreateScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var decision = await db.Decisions.SingleAsync(d => d.Id == _fixture.TestArgs.DecisionIssueId);
            decision.Type = "Focus";
            await db.SaveChangesAsync();
        }

        var restrictionTableId = Guid.NewGuid();
        var createPayload = new List<RestrictionTableIncomingDto>
        {
            new()
            {
                Id = restrictionTableId,
                ProjectId = _fixture.TestArgs.TestProjectId,
                EdgeId = _fixture.TestArgs.EdgeId,
                Name = "Repro Restriction Table",
                RestrictionEntries = new List<RestrictionEntryIncomingDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = _fixture.TestArgs.TestProjectId,
                        RestrictionTableId = restrictionTableId,
                        RestrictionValue = 1,
                        ParentStateId = _fixture.TestArgs.OptionId,
                        IsParentUncertainty = false,
                        ChildStateId = _fixture.TestArgs.OutcomeId,
                        IsChildUncertainty = true
                    }
                }
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<RestrictionTableOutgoingDto>>("restriction_tables", createPayload);
        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);

        // rebuild auto-fills the full option x outcome combination matrix (all defaulted to 1/on); toggle one entry off, like the UI does via PUT
        var afterCreate = await Client.TestClientGetAsync<RestrictionTableOutgoingDto>($"restriction_tables/{restrictionTableId}");
        var entryToToggleOff = afterCreate.Value.RestrictionEntries.Single(e => e.ParentStateId == _fixture.TestArgs.OptionId && e.ChildStateId == _fixture.TestArgs.OutcomeId);

        var updatePayload = new List<RestrictionTableIncomingDto>
        {
            new()
            {
                Id = restrictionTableId,
                ProjectId = _fixture.TestArgs.TestProjectId,
                EdgeId = _fixture.TestArgs.EdgeId,
                Name = afterCreate.Value.Name,
                RestrictionEntries = afterCreate.Value.RestrictionEntries.Select(e => new RestrictionEntryIncomingDto
                {
                    Id = e.Id,
                    ProjectId = e.ProjectId,
                    RestrictionTableId = e.RestrictionTableId,
                    RestrictionValue = e.Id == entryToToggleOff.Id ? 0 : e.RestrictionValue,
                    ParentStateId = e.ParentStateId,
                    IsParentUncertainty = e.IsParentUncertainty,
                    ChildStateId = e.ChildStateId,
                    IsChildUncertainty = e.IsChildUncertainty
                }).ToList()
            }
        };

        var updateResponse = await Client.TestClientPutAsync<List<RestrictionTableOutgoingDto>>("restriction_tables", updatePayload);
        Assert.Equal(HttpStatusCode.OK, updateResponse.Response.StatusCode);

        var optionsBefore = await Client.TestClientGetAsync<List<OptionOutgoingDto>>("options");
        var outcomesBefore = await Client.TestClientGetAsync<List<OutcomeOutgoingDto>>("outcomes");
        var beforeDuplicate = await Client.TestClientGetAsync<RestrictionTableOutgoingDto>($"restriction_tables/{restrictionTableId}");
        var originalValueByNamePair = beforeDuplicate.Value.RestrictionEntries.ToDictionary(
            e => (optionsBefore.Value.Single(o => o.Id == e.ParentStateId).Name, outcomesBefore.Value.Single(o => o.Id == e.ChildStateId).Name),
            e => e.RestrictionValue);

        // sanity check: the toggle actually took effect before duplicating
        Assert.Contains(originalValueByNamePair.Values, v => v == 0);

        var duplicateResponse = await Client.TestClientPostNoPayloadAsync<ProjectOutgoingDto>(
            $"projects/{_fixture.TestArgs.TestProjectId}/duplicate");
        Assert.Equal(HttpStatusCode.OK, duplicateResponse.Response.StatusCode);

        var optionsAfter = await Client.TestClientGetAsync<List<OptionOutgoingDto>>("options");
        var outcomesAfter = await Client.TestClientGetAsync<List<OutcomeOutgoingDto>>("outcomes");

        var allRestrictionTablesResponse = await Client.TestClientGetAsync<List<RestrictionTableOutgoingDto>>("restriction_tables");
        var duplicatedTable = Assert.Single(allRestrictionTablesResponse.Value, rt => rt.Name == "Repro Restriction Table" && rt.ProjectId != _fixture.TestArgs.TestProjectId);

        Assert.Equal(beforeDuplicate.Value.RestrictionEntries.Count, duplicatedTable.RestrictionEntries.Count);

        // the duplicated table must have at least one toggled-off (0) entry, matching the source table
        Assert.Contains(duplicatedTable.RestrictionEntries, e => e.RestrictionValue == 0);

        foreach (var entry in duplicatedTable.RestrictionEntries)
        {
            var namePair = (optionsAfter.Value.Single(o => o.Id == entry.ParentStateId).Name, outcomesAfter.Value.Single(o => o.Id == entry.ChildStateId).Name);
            Assert.True(originalValueByNamePair.TryGetValue(namePair, out var expectedValue), $"No original entry found for {namePair}");
            Assert.True(expectedValue == entry.RestrictionValue, $"{namePair}: expected={expectedValue} actual={entry.RestrictionValue}");
        }
    }

    [Fact]
    public async Task DuplicateProject_PreservesRestrictionEntryOutcomeMapping()
    {
        using var scope = _fixture.UserScope();

        // the parent decision must be a "Focus" decision, or the rebuild step will delete the restriction table as out-of-scope
        using (var setupScope = _fixture.ApiFactory.Services.CreateScope())
        {
            var db = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var decision = await db.Decisions.SingleAsync(d => d.Id == _fixture.TestArgs.DecisionIssueId);
            decision.Type = "Focus";
            await db.SaveChangesAsync();
        }

        // three distinct outcomes on the same uncertainty, each restricted to a different, distinguishable value
        var outcomeValues = new Dictionary<Guid, double>
        {
            [_fixture.TestArgs.OutcomeId] = 0.1,
            [_fixture.TestArgs.OutcomeDeleteId] = 0.5,
            [_fixture.TestArgs.OutcomeBulkDeleteId] = 0.9,
        };


        var restrictionTableId = Guid.NewGuid();
        var createPayload = new List<RestrictionTableIncomingDto>
        {
            new()
            {
                Id = restrictionTableId,
                ProjectId = _fixture.TestArgs.TestProjectId,
                EdgeId = _fixture.TestArgs.EdgeId,
                Name = "Repro Restriction Table",
                RestrictionEntries = outcomeValues.Select(kv => new RestrictionEntryIncomingDto
                {
                    Id = Guid.NewGuid(),
                    ProjectId = _fixture.TestArgs.TestProjectId,
                    RestrictionTableId = restrictionTableId,
                    RestrictionValue = kv.Value,
                    ParentStateId = _fixture.TestArgs.OptionId,
                    IsParentUncertainty = false,
                    ChildStateId = kv.Key,
                    IsChildUncertainty = true
                }).ToList()
            }
        };

        var createResponse = await Client.TestClientPostAsync<List<RestrictionTableOutgoingDto>>("restriction_tables", createPayload);
        Assert.Equal(HttpStatusCode.OK, createResponse.Response.StatusCode);

        // original (option name, outcome name) -> restriction value, captured after the auto-fill rebuild step
        var originalTable = await Client.TestClientGetAsync<RestrictionTableOutgoingDto>($"restriction_tables/{restrictionTableId}");
        var optionsBefore = await Client.TestClientGetAsync<List<OptionOutgoingDto>>("options");
        var outcomesBefore = await Client.TestClientGetAsync<List<OutcomeOutgoingDto>>("outcomes");
        var originalValueByNamePair = originalTable.Value.RestrictionEntries.ToDictionary(
            e => (optionsBefore.Value.Single(o => o.Id == e.ParentStateId).Name, outcomesBefore.Value.Single(o => o.Id == e.ChildStateId).Name),
            e => e.RestrictionValue);
        var createdAtByOutcomeName = outcomesBefore.Value.ToDictionary(o => o.Name, o => o.CreatedAt);

        var duplicateResponse = await Client.TestClientPostNoPayloadAsync<ProjectOutgoingDto>(
            $"projects/{_fixture.TestArgs.TestProjectId}/duplicate");
        Assert.Equal(HttpStatusCode.OK, duplicateResponse.Response.StatusCode);

        var optionsAfter = await Client.TestClientGetAsync<List<OptionOutgoingDto>>("options");
        var outcomesAfter = await Client.TestClientGetAsync<List<OutcomeOutgoingDto>>("outcomes");

        var allRestrictionTablesResponse = await Client.TestClientGetAsync<List<RestrictionTableOutgoingDto>>("restriction_tables");
        var duplicatedTable = Assert.Single(allRestrictionTablesResponse.Value, rt => rt.Name == "Repro Restriction Table" && rt.ProjectId != _fixture.TestArgs.TestProjectId);

        Assert.Equal(originalTable.Value.RestrictionEntries.Count, duplicatedTable.RestrictionEntries.Count);

        foreach (var entry in duplicatedTable.RestrictionEntries)
        {
            var namePair = (optionsAfter.Value.Single(o => o.Id == entry.ParentStateId).Name, outcomesAfter.Value.Single(o => o.Id == entry.ChildStateId).Name);
            Assert.True(originalValueByNamePair.TryGetValue(namePair, out var expectedValue), $"No original entry found for {namePair}");
            Assert.True(expectedValue == entry.RestrictionValue, $"{namePair}: expected={expectedValue} actual={entry.RestrictionValue}");
        }

        // duplicated outcomes must keep the source outcome's CreatedAt, or their natural display order
        // (which the app sorts by CreatedAt) shuffles relative to the original project after duplication
        foreach (var outcome in outcomesAfter.Value.Where(o => createdAtByOutcomeName.ContainsKey(o.Name)))
        {
            Assert.Equal(createdAtByOutcomeName[outcome.Name], outcome.CreatedAt);
        }
    }
}
