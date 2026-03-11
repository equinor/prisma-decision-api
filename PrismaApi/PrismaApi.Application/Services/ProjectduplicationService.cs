using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class ProjectDuplicationService : IProjectDuplicationService
{
    private readonly IProjectDuplicationRepository _duplicationRepo;
    private readonly IProjectService _projectService;
    private readonly IIssueService _issueService;
    private readonly IEdgeService _edgeService;
    private readonly IStrategyService _strategyService;
    private readonly IDiscreteProbabilityService _discreteProbabilityService;
    private readonly IDiscreteUtilityService _discreteUtilityService;

    public ProjectDuplicationService(
        IProjectDuplicationRepository duplicationRepo,
        IProjectService projectService,
        IIssueService issueService,
        IEdgeService edgeService,
        IStrategyService strategyService,
        IDiscreteProbabilityService discreteProbabilityService,
        IDiscreteUtilityService discreteUtilityService)
    {
        _duplicationRepo = duplicationRepo;
        _projectService = projectService;
        _issueService = issueService;
        _edgeService = edgeService;
        _strategyService = strategyService;
        _discreteProbabilityService = discreteProbabilityService;
        _discreteUtilityService = discreteUtilityService;
    }

    public async Task<ProjectOutgoingDto> DuplicateAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default)
    {
        var fullProject = await _duplicationRepo.GetFullProjectForDuplicationAsync(projectId, user, ct);
        if (fullProject is null)
            throw new KeyNotFoundException($"Project {projectId} not found or access denied.");

        var mappings = new IdMappings();
        var newProjectId = Guid.NewGuid();
        mappings.Project.Add(fullProject.Id, newProjectId);
        GenerateIdMappings(fullProject.Issues, mappings);

        var createProjectDto = CreateProjectDto(fullProject, newProjectId);
        var createdProjects = await _projectService.CreateAsync([createProjectDto], user);
        var createdProject = createdProjects[0];

        var issueDtos = new List<IssueIncomingDto>();
        var discreteProbabilityDtos = new List<DiscreteProbabilityDto>();
        var discreteUtilityDtos = new List<DiscreteUtilityDto>();

        foreach (var issue in fullProject.Issues)
        {
            ct.ThrowIfCancellationRequested();
            var mappedIssueId = GetMappedOrThrow(mappings.Issue, issue.Id, "issue");

            var uncertaintyResult = CreateUncertainty(issue, mappedIssueId, mappings);
            var utilityResult = CreateUtility(issue, mappedIssueId, mappings);

            if (uncertaintyResult.DiscreteProbabilities.Count > 0)
                discreteProbabilityDtos.AddRange(uncertaintyResult.DiscreteProbabilities);

            if (utilityResult.DiscreteUtilities.Count > 0)
                discreteUtilityDtos.AddRange(utilityResult.DiscreteUtilities);

            issueDtos.Add(new IssueIncomingDto
            {
                Id = mappedIssueId,
                ProjectId = newProjectId,
                Name = issue.Name,
                Description = issue.Description,
                Order = issue.Order,
                Type = issue.Type,
                Boundary = issue.Boundary,
                Node = CreateNode(issue, newProjectId, mappedIssueId, mappings),
                Decision = CreateDecision(issue, mappedIssueId, mappings),
                Uncertainty = uncertaintyResult.Uncertainty,
                Utility = utilityResult.Utility
            });
        }

        if (issueDtos.Count > 0)
            await _issueService.CreateAsync(issueDtos, user);

        if (discreteProbabilityDtos.Count > 0)
            await _discreteProbabilityService.CreateAsync(discreteProbabilityDtos);

        if (discreteUtilityDtos.Count > 0)
            await _discreteUtilityService.CreateAsync(discreteUtilityDtos);

        var strategyDtos = CreateStrategies(fullProject.Strategies, newProjectId, mappings);
        if (strategyDtos.Count > 0)
            await _strategyService.CreateAsync(strategyDtos, user);

        var edgeDtos = CreateEdges(fullProject.Edges, newProjectId, mappings);
        if (edgeDtos.Count > 0)
            await _edgeService.CreateAsync(edgeDtos);

        return createdProject;
    }

    public async Task<ProjectOutgoingDto?> DuplicateImportedProjectAsync(ProjectImportDto dto, UserOutgoingDto user, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        var mappings = new IdMappings();
        var newProjectId = Guid.NewGuid();
        var importedProjectId = dto.Projects.Id == Guid.Empty ? Guid.NewGuid() : dto.Projects.Id;
        mappings.Project[importedProjectId] = newProjectId;

        var createProjectDto = CreateProjectDtoFromImport(dto.Projects, newProjectId);
        var createdProjects = await _projectService.CreateAsync([createProjectDto], user);
        if (createdProjects.Count == 0)
            return null;

        GenerateIdMappingsFromIncoming(dto.Issues, mappings);

        var issueDtos = new List<IssueIncomingDto>();
        var discreteProbabilityDtos = new List<DiscreteProbabilityDto>();
        var discreteUtilityDtos = new List<DiscreteUtilityDto>();

        foreach (var issue in dto.Issues)
        {
            ct.ThrowIfCancellationRequested();
            var mappedIssueId = GetMappedOrThrow(mappings.Issue, issue.Id, "issue");

            var uncertaintyResult = CreateUncertaintyFromIncoming(issue, mappedIssueId, mappings);
            var utilityResult = CreateUtilityFromIncoming(issue, mappedIssueId, mappings);

            if (uncertaintyResult.DiscreteProbabilities.Count > 0)
                discreteProbabilityDtos.AddRange(uncertaintyResult.DiscreteProbabilities);

            if (utilityResult.DiscreteUtilities.Count > 0)
                discreteUtilityDtos.AddRange(utilityResult.DiscreteUtilities);

            issueDtos.Add(new IssueIncomingDto
            {
                Id = mappedIssueId,
                ProjectId = newProjectId,
                Name = issue.Name,
                Description = issue.Description,
                Order = issue.Order,
                Type = issue.Type,
                Boundary = issue.Boundary,
                Node = CreateNodeFromIncoming(issue, newProjectId, mappedIssueId, mappings),
                Decision = CreateDecisionFromIncoming(issue, mappedIssueId, mappings),
                Uncertainty = uncertaintyResult.Uncertainty,
                Utility = utilityResult.Utility
            });
        }

        if (issueDtos.Count > 0)
            await _issueService.CreateAsync(issueDtos, user);

        if (discreteProbabilityDtos.Count > 0)
            await _discreteProbabilityService.CreateAsync(discreteProbabilityDtos);

        if (discreteUtilityDtos.Count > 0)
            await _discreteUtilityService.CreateAsync(discreteUtilityDtos);

        var strategyDtos = CreateStrategiesFromIncoming(dto.Projects.Strategies, newProjectId, mappings);
        if (strategyDtos.Count > 0)
            await _strategyService.CreateAsync(strategyDtos, user);

        var edgeDtos = CreateEdgesFromIncoming(dto.Edges, newProjectId, mappings);
        if (edgeDtos.Count > 0)
            await _edgeService.CreateAsync(edgeDtos);

        return createdProjects[0];
    }

    private static ProjectCreateDto CreateProjectDto(FullProjectForDuplicationDto fullProject, Guid newProjectId)
    {
        return new ProjectCreateDto
        {
            Id = newProjectId,
            Name = fullProject.Name,
            ParentProjectId = fullProject.Id,
            ParentProjectName = fullProject.Id.ToString(),
            OpportunityStatement = fullProject.OpportunityStatement,
            Public = fullProject.Public,
            EndDate = fullProject.EndDate,
            Objectives = fullProject.Objectives
                .Select(o => new ObjectiveViaProjectDto
                {
                    Id = Guid.NewGuid(),
                    Name = o.Name,
                    Description = o.Description,
                    Type = o.Type
                })
                .ToList(),
            Users = fullProject.Users
                .Select(u => new ProjectRoleCreateDto
                {
                    UserId = u.UserId,
                    ProjectId = newProjectId,
                    Role = u.Role,
                    Name = u.Name,
                    AzureId = u.AzureId
                })
                .ToList()
        };
    }

    private static ProjectCreateDto CreateProjectDtoFromImport(ProjectIncomingDto project, Guid newProjectId)
    {
        return new ProjectCreateDto
        {
            Id = newProjectId,
            Name = project.Name,
            ParentProjectId = null,
            ParentProjectName = string.Empty,
            OpportunityStatement = project.OpportunityStatement,
            Public = project.Public,
            EndDate = project.EndDate,
            Objectives = project.Objectives
                .Select(o => new ObjectiveViaProjectDto
                {
                    Id = Guid.NewGuid(),
                    Name = o.Name,
                    Description = o.Description,
                    Type = o.Type
                })
                .ToList(),
            Users = project.Users
                .Select(u => new ProjectRoleCreateDto
                {
                    UserId = u.UserId,
                    ProjectId = newProjectId,
                    Role = u.Role,
                    Name = u.Name,
                    AzureId = u.AzureId
                })
                .ToList()
        };
    }

    private static DecisionIncomingDto? CreateDecisionFromIncoming(IssueIncomingDto issue, Guid mappedIssueId, IdMappings mappings)
    {
        if (issue.Decision is null)
            return null;

        var mappedDecisionId = GetMappedOrThrow(mappings.Decision, issue.Decision.Id, "decision");
        return new DecisionIncomingDto
        {
            Id = mappedDecisionId,
            IssueId = mappedIssueId,
            Type = issue.Decision.Type,
            Options = issue.Decision.Options
                .Select(option => new OptionIncomingDto
                {
                    Id = GetMappedOrThrow(mappings.Option, option.Id, "option"),
                    DecisionId = mappedDecisionId,
                    Name = option.Name,
                    Utility = option.Utility
                })
                .ToList()
        };
    }

    private static NodeIncomingDto? CreateNodeFromIncoming(IssueIncomingDto issue, Guid newProjectId, Guid mappedIssueId, IdMappings mappings)
    {
        if (issue.Node is null || issue.Node.NodeStyle is null)
            return null;

        var mappedNodeId = GetMappedOrThrow(mappings.Node, issue.Node.Id, "node");
        return new NodeIncomingDto
        {
            Id = mappedNodeId,
            ProjectId = newProjectId,
            IssueId = mappedIssueId,
            Name = string.IsNullOrWhiteSpace(issue.Node.Name) ? issue.Name : issue.Node.Name,
            NodeStyle = new NodeStyleIncomingDto
            {
                Id = Guid.NewGuid(),
                NodeId = mappedNodeId,
                XPosition = issue.Node.NodeStyle.XPosition,
                YPosition = issue.Node.NodeStyle.YPosition
            }
        };
    }

    private static (UncertaintyIncomingDto? Uncertainty, List<DiscreteProbabilityDto> DiscreteProbabilities) CreateUncertaintyFromIncoming(
        IssueIncomingDto issue,
        Guid mappedIssueId,
        IdMappings mappings)
    {
        if (issue.Uncertainty is null)
            return (null, new List<DiscreteProbabilityDto>());

        var mappedUncertaintyId = GetMappedOrThrow(mappings.Uncertainty, issue.Uncertainty.Id, "uncertainty");
        var outcomes = issue.Uncertainty.Outcomes
            .Select(outcome => new OutcomeIncomingDto
            {
                Id = GetMappedOrThrow(mappings.Outcome, outcome.Id, "outcome"),
                Name = outcome.Name,
                UncertaintyId = mappedUncertaintyId,
                Utility = outcome.Utility
            })
            .ToList();

        var discreteProbabilities = issue.Uncertainty.DiscreteProbabilities
            .Select(dp =>
            {
                var (parentOutcomeIds, parentOptionIds) = MapParentIds(dp.ParentOutcomeIds, dp.ParentOptionIds, mappings);
                return new DiscreteProbabilityDto
                {
                    Id = Guid.NewGuid(),
                    UncertaintyId = mappedUncertaintyId,
                    OutcomeId = mappings.Outcome.GetValueOrDefault(dp.OutcomeId, dp.OutcomeId),
                    Probability = dp.Probability,
                    ParentOutcomeIds = parentOutcomeIds,
                    ParentOptionIds = parentOptionIds
                };
            })
            .ToList();

        var uncertaintyDto = new UncertaintyIncomingDto
        {
            Id = mappedUncertaintyId,
            IssueId = mappedIssueId,
            IsKey = issue.Uncertainty.IsKey,
            Outcomes = outcomes,
            DiscreteProbabilities = new List<DiscreteProbabilityDto>()
        };

        return (uncertaintyDto, discreteProbabilities);
    }

    private static (UtilityIncomingDto? Utility, List<DiscreteUtilityDto> DiscreteUtilities) CreateUtilityFromIncoming(
        IssueIncomingDto issue,
        Guid mappedIssueId,
        IdMappings mappings)
    {
        if (issue.Utility is null)
            return (null, new List<DiscreteUtilityDto>());

        var mappedUtilityId = GetMappedOrThrow(mappings.Utility, issue.Utility.Id, "utility");

        var utilityDto = new UtilityIncomingDto
        {
            Id = mappedUtilityId,
            IssueId = mappedIssueId
        };

        return (utilityDto, new List<DiscreteUtilityDto>());
    }

    private static DecisionIncomingDto? CreateDecision(IssueOutgoingDto issue, Guid mappedIssueId, IdMappings mappings)
    {
        if (issue.Decision is null)
            return null;

        var mappedDecisionId = GetMappedOrThrow(mappings.Decision, issue.Decision.Id, "decision");
        return new DecisionIncomingDto
        {
            Id = mappedDecisionId,
            IssueId = mappedIssueId,
            Type = issue.Decision.Type,
            Options = issue.Decision.Options
                .Select(option => new OptionIncomingDto
                {
                    Id = GetMappedOrThrow(mappings.Option, option.Id, "option"),
                    DecisionId = mappedDecisionId,
                    Name = option.Name,
                    Utility = option.Utility
                })
                .ToList()
        };
    }

    private static NodeIncomingDto? CreateNode(IssueOutgoingDto issue, Guid newProjectId, Guid mappedIssueId, IdMappings mappings)
    {
        if (issue.Node is null || issue.Node.NodeStyle is null)
            return null;

        var mappedNodeId = GetMappedOrThrow(mappings.Node, issue.Node.Id, "node");
        return new NodeIncomingDto
        {
            Id = mappedNodeId,
            ProjectId = newProjectId,
            IssueId = mappedIssueId,
            Name = string.IsNullOrWhiteSpace(issue.Node.Name) ? issue.Name : issue.Node.Name,
            NodeStyle = new NodeStyleIncomingDto
            {
                Id = Guid.NewGuid(),
                NodeId = mappedNodeId,
                XPosition = issue.Node.NodeStyle.XPosition,
                YPosition = issue.Node.NodeStyle.YPosition
            }
        };
    }

    private static (UncertaintyIncomingDto? Uncertainty, List<DiscreteProbabilityDto> DiscreteProbabilities) CreateUncertainty(
        IssueOutgoingDto issue,
        Guid mappedIssueId,
        IdMappings mappings)
    {
        if (issue.Uncertainty is null)
            return (null, new List<DiscreteProbabilityDto>());

        var mappedUncertaintyId = GetMappedOrThrow(mappings.Uncertainty, issue.Uncertainty.Id, "uncertainty");
        var outcomes = issue.Uncertainty.Outcomes
            .Select(outcome => new OutcomeIncomingDto
            {
                Id = GetMappedOrThrow(mappings.Outcome, outcome.Id, "outcome"),
                Name = outcome.Name,
                UncertaintyId = mappedUncertaintyId,
                Utility = outcome.Utility
            })
            .ToList();

        var discreteProbabilities = issue.Uncertainty.DiscreteProbabilities
            .Select(dp =>
            {
                var (parentOutcomeIds, parentOptionIds) = MapParentIds(dp.ParentOutcomeIds, dp.ParentOptionIds, mappings);
                return new DiscreteProbabilityDto
                {
                    Id = Guid.NewGuid(),
                    UncertaintyId = mappedUncertaintyId,
                    OutcomeId = mappings.Outcome.GetValueOrDefault(dp.OutcomeId, dp.OutcomeId),
                    Probability = dp.Probability,
                    ParentOutcomeIds = parentOutcomeIds,
                    ParentOptionIds = parentOptionIds
                };
            })
            .ToList();

        var uncertaintyDto = new UncertaintyIncomingDto
        {
            Id = mappedUncertaintyId,
            IssueId = mappedIssueId,
            IsKey = issue.Uncertainty.IsKey,
            Outcomes = outcomes,
            DiscreteProbabilities = new List<DiscreteProbabilityDto>()
        };

        return (uncertaintyDto, discreteProbabilities);
    }

    private static (UtilityIncomingDto? Utility, List<DiscreteUtilityDto> DiscreteUtilities) CreateUtility(
        IssueOutgoingDto issue,
        Guid mappedIssueId,
        IdMappings mappings)
    {
        if (issue.Utility is null)
            return (null, new List<DiscreteUtilityDto>());

        var mappedUtilityId = GetMappedOrThrow(mappings.Utility, issue.Utility.Id, "utility");

        var discreteUtilities = issue.Utility.DiscreteUtilities
            .Select(du =>
            {
                var (parentOutcomeIds, parentOptionIds) = MapParentIds(du.ParentOutcomeIds, du.ParentOptionIds, mappings);
                return new DiscreteUtilityDto
                {
                    Id = Guid.NewGuid(),
                    UtilityId = mappedUtilityId,
                    UtilityValue = du.UtilityValue,
                    ParentOptionIds = parentOptionIds,
                    ParentOutcomeIds = parentOutcomeIds,
                    ValueMetricId = du.ValueMetricId
                };
            })
            .ToList();

        var utilityDto = new UtilityIncomingDto
        {
            Id = mappedUtilityId,
            IssueId = mappedIssueId
        };

        return (utilityDto, discreteUtilities);
    }

    private static List<StrategyIncomingDto> CreateStrategies(
        List<StrategyOutgoingDto> strategies,
        Guid newProjectId,
        IdMappings mappings)
    {
        return strategies.Select(strategy => new StrategyIncomingDto
        {
            Id = Guid.NewGuid(),
            ProjectId = newProjectId,
            Name = strategy.Name,
            Description = strategy.Description,
            Rationale = strategy.Rationale,
            Icon = strategy.Icon,
            IconColor = strategy.IconColor,
            Options = strategy.Options.Select(option => new OptionIncomingDto
            {
                Id = GetMappedOrThrow(mappings.Option, option.Id, "option"),
                DecisionId = GetMappedOrThrow(mappings.Decision, option.DecisionId, "decision"),
                Name = option.Name,
                Utility = option.Utility
            }).ToList()
        }).ToList();
    }

    private static List<StrategyIncomingDto> CreateStrategiesFromIncoming(
        List<StrategyIncomingDto> strategies,
        Guid newProjectId,
        IdMappings mappings)
    {
        return strategies.Select(strategy => new StrategyIncomingDto
        {
            Id = Guid.NewGuid(),
            ProjectId = newProjectId,
            Name = strategy.Name,
            Description = strategy.Description,
            Rationale = strategy.Rationale,
            Icon = strategy.Icon,
            IconColor = strategy.IconColor,
            Options = strategy.Options.Select(option => new OptionIncomingDto
            {
                Id = GetMappedOrThrow(mappings.Option, option.Id, "option"),
                DecisionId = GetMappedOrThrow(mappings.Decision, option.DecisionId, "decision"),
                Name = option.Name,
                Utility = option.Utility
            }).ToList()
        }).ToList();
    }

    private static List<EdgeIncomingDto> CreateEdges(List<EdgeOutgoingDto> edges, Guid newProjectId, IdMappings mappings)
    {
        return edges.Select(edge => new EdgeIncomingDto
        {
            Id = Guid.NewGuid(),
            TailId = GetMappedOrThrow(mappings.Node, edge.TailId, "node"),
            HeadId = GetMappedOrThrow(mappings.Node, edge.HeadId, "node"),
            ProjectId = newProjectId
        }).ToList();
    }

    private static List<EdgeIncomingDto> CreateEdgesFromIncoming(List<EdgeIncomingDto> edges, Guid newProjectId, IdMappings mappings)
    {
        return edges.Select(edge => new EdgeIncomingDto
        {
            Id = Guid.NewGuid(),
            TailId = GetMappedOrThrow(mappings.Node, edge.TailId, "node"),
            HeadId = GetMappedOrThrow(mappings.Node, edge.HeadId, "node"),
            ProjectId = newProjectId
        }).ToList();
    }

    private static void GenerateIdMappings(List<IssueOutgoingDto> issues, IdMappings mappings)
    {
        foreach (var issue in issues)
        {
            mappings.Issue[issue.Id] = Guid.NewGuid();

            if (issue.Node is not null)
                mappings.Node[issue.Node.Id] = Guid.NewGuid();

            if (issue.Uncertainty is not null)
            {
                mappings.Uncertainty[issue.Uncertainty.Id] = Guid.NewGuid();
                foreach (var outcome in issue.Uncertainty.Outcomes)
                    mappings.Outcome[outcome.Id] = Guid.NewGuid();
            }

            if (issue.Decision is not null)
            {
                mappings.Decision[issue.Decision.Id] = Guid.NewGuid();
                foreach (var option in issue.Decision.Options)
                    mappings.Option[option.Id] = Guid.NewGuid();
            }

            if (issue.Utility is not null)
                mappings.Utility[issue.Utility.Id] = Guid.NewGuid();
        }
    }

    private static void GenerateIdMappingsFromIncoming(List<IssueIncomingDto> issues, IdMappings mappings)
    {
        foreach (var issue in issues)
        {
            mappings.Issue[issue.Id] = Guid.NewGuid();

            if (issue.Node is not null)
                mappings.Node[issue.Node.Id] = Guid.NewGuid();

            if (issue.Uncertainty is not null)
            {
                mappings.Uncertainty[issue.Uncertainty.Id] = Guid.NewGuid();
                foreach (var outcome in issue.Uncertainty.Outcomes)
                    mappings.Outcome[outcome.Id] = Guid.NewGuid();
            }

            if (issue.Decision is not null)
            {
                mappings.Decision[issue.Decision.Id] = Guid.NewGuid();
                foreach (var option in issue.Decision.Options)
                    mappings.Option[option.Id] = Guid.NewGuid();
            }

            if (issue.Utility is not null)
                mappings.Utility[issue.Utility.Id] = Guid.NewGuid();
        }
    }

    private static (List<Guid> ParentOutcomeIds, List<Guid> ParentOptionIds) MapParentIds(
        List<Guid> parentOutcomeIds,
        List<Guid> parentOptionIds,
        IdMappings mappings)
    {
        var mappedOutcomeIds = parentOutcomeIds
            .Where(mappings.Outcome.ContainsKey)
            .Select(id => mappings.Outcome[id])
            .ToList();

        var mappedOptionIds = parentOptionIds
            .Where(mappings.Option.ContainsKey)
            .Select(id => mappings.Option[id])
            .ToList();

        return (mappedOutcomeIds, mappedOptionIds);
    }

    private static Guid GetMappedOrThrow(Dictionary<Guid, Guid> map, Guid key, string mapName)
    {
        if (!map.TryGetValue(key, out var value))
            throw new InvalidOperationException($"Missing {mapName} mapping for id '{key}'.");

        return value;
    }

    private sealed class IdMappings
    {
        public Dictionary<Guid, Guid> Project { get; } = new();
        public Dictionary<Guid, Guid> Issue { get; } = new();
        public Dictionary<Guid, Guid> Uncertainty { get; } = new();
        public Dictionary<Guid, Guid> Decision { get; } = new();
        public Dictionary<Guid, Guid> Utility { get; } = new();
        public Dictionary<Guid, Guid> Node { get; } = new();
        public Dictionary<Guid, Guid> Outcome { get; } = new();
        public Dictionary<Guid, Guid> Option { get; } = new();
    }
}
