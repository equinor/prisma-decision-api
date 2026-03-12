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
        GenerateIdMappings(
            fullProject.Issues,
            i => i.Node?.Id,
            i => i.Uncertainty is null ? null : (i.Uncertainty.Id, i.Uncertainty.Outcomes.Select(o => o.Id)),
            i => i.Decision is null ? null : (i.Decision.Id, i.Decision.Options.Select(o => o.Id)),
            i => i.Utility?.Id,
            mappings);

        var createProjectDto = CreateProjectDto(fullProject, newProjectId);
        var createdProjects = await _projectService.CreateAsync([createProjectDto], isProjectNotDuplicated: false, userDto: user);
        var createdProject = createdProjects[0];

        var issueDtos = new List<IssueIncomingDto>();
        var discreteProbabilityDtos = new List<DiscreteProbabilityDto>();
        var discreteUtilityDtos = new List<DiscreteUtilityDto>();

        foreach (var issue in fullProject.Issues)
        {
            ct.ThrowIfCancellationRequested();
            var mappedIssueId = GetMappedOrThrow(mappings.Issue, issue.Id, "issue");

            var uncertaintyResult = CreateUncertainty(
                issue.Uncertainty?.Id, issue.Uncertainty?.IsKey ?? true,
                issue.Uncertainty?.Outcomes, issue.Uncertainty?.DiscreteProbabilities,
                mappedIssueId, mappings);
            var utilityResult = CreateUtility(
                issue.Utility?.Id, issue.Utility?.DiscreteUtilities, mappedIssueId, mappings);

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
                Node = CreateNode(issue.Node?.Id, issue.Node?.Name, issue.Node?.NodeStyle?.XPosition, issue.Node?.NodeStyle?.YPosition, issue.Name, newProjectId, mappedIssueId, mappings),
                Decision = CreateDecision(issue.Decision?.Id, issue.Decision?.Type, issue.Decision?.Options, mappedIssueId, mappings),
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

        var strategyDtos = CreateStrategies(fullProject.Strategies, s => s.Options, newProjectId, mappings);
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
        var createdProjects = await _projectService.CreateAsync([createProjectDto], isProjectNotDuplicated: false, userDto: user);
        if (createdProjects.Count == 0)
            return null;

        GenerateIdMappings(
            dto.Issues,
            i => i.Node?.Id,
            i => i.Uncertainty is null ? null : (i.Uncertainty.Id, i.Uncertainty.Outcomes.Select(o => o.Id)),
            i => i.Decision is null ? null : (i.Decision.Id, i.Decision.Options.Select(o => o.Id)),
            i => i.Utility?.Id,
            mappings);

        var issueDtos = new List<IssueIncomingDto>();
        var discreteProbabilityDtos = new List<DiscreteProbabilityDto>();
        var discreteUtilityDtos = new List<DiscreteUtilityDto>();

        foreach (var issue in dto.Issues)
        {
            ct.ThrowIfCancellationRequested();
            var mappedIssueId = GetMappedOrThrow(mappings.Issue, issue.Id, "issue");

            var uncertaintyResult = CreateUncertainty(
                issue.Uncertainty?.Id, issue.Uncertainty?.IsKey ?? true,
                issue.Uncertainty?.Outcomes, issue.Uncertainty?.DiscreteProbabilities,
                mappedIssueId, mappings);
            var utilityResult = CreateUtility(
                issue.Utility?.Id, issue.Utility?.DiscreteUtilities,
                mappedIssueId, mappings);

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
                Node = CreateNode(issue.Node?.Id, issue.Node?.Name, issue.Node?.NodeStyle?.XPosition, issue.Node?.NodeStyle?.YPosition, issue.Name, newProjectId, mappedIssueId, mappings),
                Decision = CreateDecision(issue.Decision?.Id, issue.Decision?.Type, issue.Decision?.Options, mappedIssueId, mappings),
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

        var strategyDtos = CreateStrategies(dto.Projects.Strategies, s => s.Options, newProjectId, mappings);
        if (strategyDtos.Count > 0)
            await _strategyService.CreateAsync(strategyDtos, user);

        var edgeDtos = CreateEdges(dto.Edges, newProjectId, mappings);
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

    private static DecisionIncomingDto? CreateDecision(
        Guid? decisionId,
        string? decisionType,
        IEnumerable<OptionDto>? options,
        Guid mappedIssueId,
        IdMappings mappings)
    {
        if (decisionId is null)
            return null;

        var mappedDecisionId = GetMappedOrThrow(mappings.Decision, decisionId.Value, "decision");
        return new DecisionIncomingDto
        {
            Id = mappedDecisionId,
            IssueId = mappedIssueId,
            Type = decisionType ?? "Focus",
            Options = (options ?? [])
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

    private static NodeIncomingDto? CreateNode(
        Guid? sourceNodeId,
        string? sourceNodeName,
        double? xPosition,
        double? yPosition,
        string issueName,
        Guid newProjectId,
        Guid mappedIssueId,
        IdMappings mappings)
    {
        if (sourceNodeId is null || xPosition is null || yPosition is null)
            return null;

        var mappedNodeId = GetMappedOrThrow(mappings.Node, sourceNodeId.Value, "node");
        return new NodeIncomingDto
        {
            Id = mappedNodeId,
            ProjectId = newProjectId,
            IssueId = mappedIssueId,
            Name = string.IsNullOrWhiteSpace(sourceNodeName) ? issueName : sourceNodeName,
            NodeStyle = new NodeStyleIncomingDto
            {
                Id = Guid.NewGuid(),
                NodeId = mappedNodeId,
                XPosition = xPosition.Value,
                YPosition = yPosition.Value
            }
        };
    }

    private static (UncertaintyIncomingDto? Uncertainty, List<DiscreteProbabilityDto> DiscreteProbabilities) CreateUncertainty(
        Guid? uncertaintyId,
        bool isKey,
        IEnumerable<OutcomeDto>? outcomes,
        IEnumerable<DiscreteProbabilityDto>? discreteProbabilities,
        Guid mappedIssueId,
        IdMappings mappings)
    {
        if (uncertaintyId is null)
            return (null, []);

        var mappedUncertaintyId = GetMappedOrThrow(mappings.Uncertainty, uncertaintyId.Value, "uncertainty");
        var mappedOutcomes = (outcomes ?? [])
            .Select(outcome => new OutcomeIncomingDto
            {
                Id = GetMappedOrThrow(mappings.Outcome, outcome.Id, "outcome"),
                Name = outcome.Name,
                UncertaintyId = mappedUncertaintyId,
                Utility = outcome.Utility
            })
            .ToList();

        var mappedDiscreteProbabilities = (discreteProbabilities ?? [])
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
            IsKey = isKey,
            Outcomes = mappedOutcomes,
            DiscreteProbabilities = []
        };

        return (uncertaintyDto, mappedDiscreteProbabilities);
    }

    private static (UtilityIncomingDto? Utility, List<DiscreteUtilityDto> DiscreteUtilities) CreateUtility(
        Guid? utilityId,
        IEnumerable<DiscreteUtilityDto>? discreteUtilities,
        Guid mappedIssueId,
        IdMappings mappings)
    {
        if (utilityId is null)
            return (null, []);

        var mappedUtilityId = GetMappedOrThrow(mappings.Utility, utilityId.Value, "utility");
        var utilityDto = new UtilityIncomingDto
        {
            Id = mappedUtilityId,
            IssueId = mappedIssueId
        };

        var mappedDiscreteUtilities = (discreteUtilities ?? [])
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

        return (utilityDto, mappedDiscreteUtilities);
    }

    private static List<StrategyIncomingDto> CreateStrategies<TStrategy>(
        IEnumerable<TStrategy> strategies,
        Func<TStrategy, IEnumerable<OptionDto>> getOptions,
        Guid newProjectId,
        IdMappings mappings) where TStrategy : StrategyDto
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
            Options = getOptions(strategy).Select(option => new OptionIncomingDto
            {
                Id = GetMappedOrThrow(mappings.Option, option.Id, "option"),
                DecisionId = GetMappedOrThrow(mappings.Decision, option.DecisionId, "decision"),
                Name = option.Name,
                Utility = option.Utility
            }).ToList()
        }).ToList();
    }

    private static List<EdgeIncomingDto> CreateEdges(IEnumerable<EdgeDto> edges, Guid newProjectId, IdMappings mappings)
    {
        return edges.Select(edge => new EdgeIncomingDto
        {
            Id = Guid.NewGuid(),
            TailId = GetMappedOrThrow(mappings.Node, edge.TailId, "node"),
            HeadId = GetMappedOrThrow(mappings.Node, edge.HeadId, "node"),
            ProjectId = newProjectId
        }).ToList();
    }

    private static void GenerateIdMappings<TIssue>(
        IEnumerable<TIssue> issues,
        Func<TIssue, Guid?> getNodeId,
        Func<TIssue, (Guid Id, IEnumerable<Guid> OutcomeIds)?> getUncertaintyInfo,
        Func<TIssue, (Guid Id, IEnumerable<Guid> OptionIds)?> getDecisionInfo,
        Func<TIssue, Guid?> getUtilityId,
        IdMappings mappings) where TIssue : IssueDto
    {
        foreach (var issue in issues)
        {
            mappings.Issue[issue.Id] = Guid.NewGuid();

            var nodeId = getNodeId(issue);
            if (nodeId.HasValue)
                mappings.Node[nodeId.Value] = Guid.NewGuid();

            var uncertaintyInfo = getUncertaintyInfo(issue);
            if (uncertaintyInfo.HasValue)
            {
                mappings.Uncertainty[uncertaintyInfo.Value.Id] = Guid.NewGuid();
                foreach (var outcomeId in uncertaintyInfo.Value.OutcomeIds)
                    mappings.Outcome[outcomeId] = Guid.NewGuid();
            }

            var decisionInfo = getDecisionInfo(issue);
            if (decisionInfo.HasValue)
            {
                mappings.Decision[decisionInfo.Value.Id] = Guid.NewGuid();
                foreach (var optionId in decisionInfo.Value.OptionIds)
                    mappings.Option[optionId] = Guid.NewGuid();
            }

            var utilityId = getUtilityId(issue);
            if (utilityId.HasValue)
                mappings.Utility[utilityId.Value] = Guid.NewGuid();
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
