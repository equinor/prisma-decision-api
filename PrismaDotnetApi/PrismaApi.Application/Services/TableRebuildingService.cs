using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Domain.Extensions;
using PrismaApi.Domain.Utilities;
using PrismaDotnetApi.PrismaApi.Domain.Extensions;

namespace PrismaApi.Application.Services;

public class TableRebuildingService : ITableRebuildingService
{
    protected readonly AppDbContext DbContext;
    public TableRebuildingService(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    private static Guid GetDeterministicIdRestrictionEntry(Guid restrictionTableId, Guid parentId, Guid childId)
    {
        var combined = $"{restrictionTableId}|" +
                       $"ParentId:{parentId}|" +
                       $"ChildId:{childId}";
        return combined.GenerateDeterministicGuid();
    }

    public async Task RebuildTablesAsync(CancellationToken ct = default)
    {
        var issueIds = DbContext.DiscreteTableSessionInfo.AffectedIssueIds.ToHashSet();
        if (issueIds.Count != 0 && !DbContext.IsDiscreteTableEventDisabled)
        {
            await RebuildIssuesFromIssueIds(issueIds, ct);
        }
        var restrictionTableIds = DbContext.DiscreteTableSessionInfo.AffectedRestrictionTables.ToHashSet();
        if (restrictionTableIds.Count != 0 && !DbContext.IsDiscreteTableEventDisabled)
        {
            await RebuildRestrictionTablesFromIds(restrictionTableIds, ct);
        }
        await DbContext.SaveChangesAsync(ct);
        DbContext.DiscreteTableSessionInfo.Clear();
    }

    private async Task RebuildRestrictionTablesFromIds(HashSet<Guid> restrictionTableIds, CancellationToken ct)
    {
        // the data we need is the edges connected to the issues that are parents of the restriction tables, so we can determine the new combinations of parent outcomes and options that should be in the restriction table, then we can delete the existing entries and add the new ones
        // we can get the outcomes and options connected without joining from the restriction table, the issue information is still needed to determine the parent issues and their types, but we can get that with a separate query and then do the rest of the processing in memory
        var restrictionTables = await DbContext.RestrictionTables
            .AsSplitQuery()
            .Where(rt => restrictionTableIds.Contains(rt.Id))
            .Include(rt => rt.RestrictionEntries)
            .Include(rt => rt.Edge)
                .ThenInclude(edge => edge!.HeadNode)
                    .ThenInclude(node => node!.Issue)
                        .ThenInclude(issue => issue!.Uncertainty)
                            .ThenInclude(uncertainty => uncertainty!.Outcomes)
            .Include(rt => rt.Edge)
                .ThenInclude(edge => edge!.HeadNode)
                    .ThenInclude(node => node!.Issue)
                        .ThenInclude(issue => issue!.Decision)
                            .ThenInclude(decision => decision!.Options)
            .Include(rt => rt.Edge)
                .ThenInclude(edge => edge!.TailNode)
                    .ThenInclude(node => node!.Issue)
                        .ThenInclude(issue => issue!.Uncertainty)
                            .ThenInclude(uncertainty => uncertainty!.Outcomes)
            .Include(rt => rt.Edge)
                .ThenInclude(edge => edge!.TailNode)
                    .ThenInclude(node => node!.Issue)
                        .ThenInclude(issue => issue!.Decision)
                            .ThenInclude(decision => decision!.Options)
            .ToListAsync(ct);

        foreach (var restrictionTable in restrictionTables)
        {
            await RebuildRestrictionTable(restrictionTable, ct);
        }
    }

    private async Task RebuildRestrictionTable(RestrictionTable restrictionTable, CancellationToken ct)
    {
        var parentIssue = restrictionTable.Edge!.TailNode!.Issue!;
        var childIssue = restrictionTable.Edge!.HeadNode!.Issue!;
        var isParentUncertainty = IsIssueType(parentIssue.Type, IssueType.Uncertainty) && parentIssue.Uncertainty != null;
        var isChildUncertainty = IsIssueType(childIssue.Type, IssueType.Uncertainty) && childIssue.Uncertainty != null;
        // check if parent and child are in scope of the influence diagram, if not the restriction table should be deleted,
        // if yes then we need to check if the combinations of parent outcomes and options have changed and update the restriction table entries accordingly
        if (!IsBoundaryInScope(parentIssue.Boundary) ||
            !IsBoundaryInScope(childIssue.Boundary) ||
            !IsIssueConfiguredInInfluenceDiagram(parentIssue) ||
            !IsIssueConfiguredInInfluenceDiagram(childIssue))
        {
            DbContext.RestrictionTables.Remove(restrictionTable);
            return;
        }
        // check that the parent child combination are correct,
        // this is simpler than the other tables since a restriction entry can only have one child and one parent,
        // so we just need to check that the parent issue has at least one of the relevant types and that the child issue has at least one of the relevant types,
        // if not then the restriction table should be deleted,
        // if yes then we need to check if the combinations of parent outcomes and options have changed and update the restriction table entries accordingly

        var restrictionEntries = restrictionTable.RestrictionEntries.ToList();

        List<(Guid? ParentId, Guid? ChildId)> existingCombinations = restrictionEntries
            .Select(entry => (
                ParentId: entry.ParentOutcomeId ?? entry.ParentOptionId,
                ChildId: entry.ChildOutcomeId ?? entry.ChildOptionId))
            .ToList();

        var parentIds = isParentUncertainty
            ? parentIssue.Uncertainty!.Outcomes.Select(o => (Guid?)o.Id)
            : parentIssue.Decision!.Options.Select(o => (Guid?)o.Id);

        var childIds = isChildUncertainty
            ? childIssue.Uncertainty!.Outcomes.Select(o => (Guid?)o.Id)
            : childIssue.Decision!.Options.Select(o => (Guid?)o.Id);

        List<(Guid? ParentId, Guid? ChildId)> validCombinations = (from parentId in parentIds
                                                                   from childId in childIds
                                                                   select (ParentId: parentId, ChildId: childId)).ToList();

        // check that the current combinations in the restriction table are still valid, if not then remove them, then add any new valid combinations that are not currently in the restriction table

        if (existingCombinations.All(ec => validCombinations.Contains(ec)) && existingCombinations.Count == validCombinations.Count)
        {
            // the combinations are the same, no need to update the restriction table entries
            return;
        }
        else if (existingCombinations.All(ec => validCombinations.Contains(ec)))
        {
            // the existing combinations are a subset of the valid combinations, we just need to add the new combinations
            var combinationsToAdd = validCombinations.Except(existingCombinations).ToList();
            foreach (var combination in combinationsToAdd)
            {
                var id = combination.ParentId != null && combination.ChildId != null
                    ? GetDeterministicIdRestrictionEntry(restrictionTable.Id, combination.ParentId.Value, combination.ChildId.Value)
                    : Guid.NewGuid();
                var newEntry = new RestrictionEntry
                {
                    Id = id,
                    RestrictionTableId = restrictionTable.Id,
                    ProjectId = restrictionTable.ProjectId,
                    ParentOutcomeId = isParentUncertainty ? combination.ParentId : null,
                    ParentOptionId = isParentUncertainty ? null : combination.ParentId,
                    ChildOutcomeId = isChildUncertainty ? combination.ChildId : null,
                    ChildOptionId = isChildUncertainty ? null : combination.ChildId,
                };
                await DbContext.RestrictionEntries.AddAsync(newEntry, ct);
            }
        }
        else
        {
            // some of the existing combinations are no longer valid, we need to remove the invalid combinations and add any new valid combinations
            var combinationsToRemove = existingCombinations.Except(validCombinations).ToList();
            var entriesToRemove = restrictionEntries.Where(entry =>
                combinationsToRemove.Contains((
                    ParentId: entry.ParentOutcomeId ?? entry.ParentOptionId,
                    ChildId: entry.ChildOutcomeId ?? entry.ChildOptionId))
            ).ToList();
            DbContext.RestrictionEntries.RemoveRange(entriesToRemove);

            var combinationsToAdd = validCombinations.Except(existingCombinations).ToList();
            foreach (var combination in combinationsToAdd)
            {
                var id = combination.ParentId != null && combination.ChildId != null
                    ? GetDeterministicIdRestrictionEntry(restrictionTable.Id, combination.ParentId.Value, combination.ChildId.Value)
                    : Guid.NewGuid();
                var newEntry = new RestrictionEntry
                {
                    Id = id,
                    RestrictionTableId = restrictionTable.Id,
                    ProjectId = restrictionTable.ProjectId,
                    ParentOutcomeId = isParentUncertainty ? combination.ParentId : null,
                    ParentOptionId = isParentUncertainty ? null : combination.ParentId,
                    ChildOutcomeId = isChildUncertainty ? combination.ChildId : null,
                    ChildOptionId = isChildUncertainty ? null : combination.ChildId,
                };
                await DbContext.RestrictionEntries.AddAsync(newEntry, ct);
            }
        }
    }

    public async Task RebuildIssuesFromIssueIds(ICollection<Guid> issueIds, CancellationToken ct = default)
    {
        var issues = await DbContext.Issues
            .AsSplitQuery()
            .Where(issue => issueIds.Contains(issue.Id))
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.Outcomes)
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.DiscreteProbabilities)
                    .ThenInclude(probability => probability.ParentOutcomes)
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.DiscreteProbabilities)
                    .ThenInclude(probability => probability.ParentOptions)
            .Include(issue => issue.Utility!)
                .ThenInclude(utility => utility.DiscreteUtilities)
                    .ThenInclude(utility => utility.ParentOutcomes)
            .Include(issue => issue.Utility!)
                .ThenInclude(utility => utility.DiscreteUtilities)
                    .ThenInclude(utility => utility.ParentOptions)
            .Include(issue => issue.Node!)
                .ThenInclude(node => node.HeadEdges)
                    .ThenInclude(edge => edge.TailNode!)
                        .ThenInclude(node => node.Issue!)
                            .ThenInclude(parentIssue => parentIssue.Uncertainty!)
                                .ThenInclude(uncertainty => uncertainty.Outcomes)
            .Include(issue => issue.Node!)
                .ThenInclude(node => node.HeadEdges)
                    .ThenInclude(edge => edge.TailNode!)
                        .ThenInclude(node => node.Issue!)
                            .ThenInclude(parentIssue => parentIssue.Decision!)
                                .ThenInclude(decision => decision.Options)
            .ToListAsync(ct);

        foreach (var issue in issues)
        {
            if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty != null)
            {
                await RebuildUncertaintyTable(issue, ct);
            }
            else if (IsIssueType(issue.Type, IssueType.Utility) && issue.Utility != null)
            {
                await RebuildUtilityTable(issue, ct);
            }
        }

        await DbContext.SaveChangesAsync(ct);
    }


    /// <summary>
    /// Rebuilds the uncertainty table for the specified issue.
    /// Note that the differance between the desired probabilities and the expanded probabilities is that the desired probabilities will always have the correct dimensions, while the expansion probabilities handles the prepopulation logic.
    /// </summary>
    private async Task RebuildUncertaintyTable(Issue issue, CancellationToken ct = default)
    {
        var uncertainty = issue.Uncertainty;
        if (uncertainty == null)
        {
            return;
        }
        var existingProbabilities = uncertainty.DiscreteProbabilities.ToList();
        // Convert the existing probabilities to DTOs that will be used for expansion and reconciliation.
        var expandedProbabilities = existingProbabilities.ToDtos();

        // Collect the parent state groups for the issue. 
        // These groups represent the different combinations of parent states that will influence the probabilities.
        var parentGroups = CollectParentStateGroups(issue);

        if (expandedProbabilities.Count != 0)
        {
            // Add rows to the Probability table for each combination of parent states.
            ExpandProbabilityParents(issue.Id, expandedProbabilities, parentGroups);

            // Add columns for any new outcomes that are not already represented in the expanded probabilities.
            var representedOutcomeIds = expandedProbabilities
                .Select(probability => probability.OutcomeId)
                .ToHashSet();
            foreach (var addedOutcomeId in uncertainty.Outcomes
                .Select(outcome => outcome.Id)
                .Where(outcomeId => !representedOutcomeIds.Contains(outcomeId)))
            {
                expandedProbabilities.AddProbabilitiesColumn(issue.Id, addedOutcomeId);
            }
        }

        // Build the desired probabilities that reflect the expanded parent state groups.
        // This represents the target state of the Probability table with the correct dimensions
        var desiredProbabilities = BuildDesiredProbabilities(issue, parentGroups, expandedProbabilities);
        await ReconcileProbabilities(existingProbabilities, desiredProbabilities, ct);
    }

    /// <summary>
    /// Rebuilds the utility table for the specified issue.
    /// Note that the difference between the desired utilities and the expanded utilities is that the desired utilities will always have the correct dimensions, while the expanded utilities handle the prepopulation logic.
    /// </summary>
    private async Task RebuildUtilityTable(Issue issue, CancellationToken ct = default)
    {
        var utility = issue.Utility;
        if (utility == null)
        {
            return;
        }

        var existingUtilities = utility.DiscreteUtilities.ToList();
        // Convert the existing utilities to DTOs that will be used for expansion and reconciliation.
        var expandedUtilities = existingUtilities.ToDtos();

        // Collect the parent state groups for the issue.
        // These groups represent the different combinations of parent states that will influence the utilities.
        var parentGroups = CollectParentStateGroups(issue);

        if (expandedUtilities.Count != 0)
        {
            // Add rows to the Utility table for each combination of parent states.
            ExpandUtilityParents(issue.Id, expandedUtilities, parentGroups);
        }

        // Build the desired utilities that reflect the expanded parent state groups.
        // This represents the target state of the Utility table with the correct dimensions.
        var desiredUtilities = BuildDesiredUtilities(issue, parentGroups, expandedUtilities);
        await ReconcileUtilities(existingUtilities, desiredUtilities, ct);
    }

    private static void ExpandProbabilityParents(
        Guid issueId,
        ICollection<DiscreteProbabilityDto> probabilities,
        IEnumerable<ParentStateGroup> parentGroups)
    {
        foreach (var parentGroup in parentGroups)
        {
            var representedStateIds = parentGroup.StateIds
                .Where(stateId => probabilities.Any(probability =>
                    probability.ParentOutcomeIds.Contains(stateId) ||
                    probability.ParentOptionIds.Contains(stateId)))
                .ToList();
            var addedStateIds = parentGroup.StateIds.Except(representedStateIds).ToList();

            if (representedStateIds.Count == 0)
            {
                probabilities.AddProbabilityParent(issueId, addedStateIds, parentGroup.IsUncertainty);
                continue;
            }

            foreach (var addedStateId in addedStateIds)
            {
                probabilities.AddProbabilitiesRow(issueId, addedStateId, representedStateIds);
            }
        }
    }

    private static void ExpandUtilityParents(
        Guid issueId,
        ICollection<DiscreteUtilityDto> utilities,
        IEnumerable<ParentStateGroup> parentGroups)
    {
        foreach (var parentGroup in parentGroups)
        {
            var representedStateIds = parentGroup.StateIds
                .Where(stateId => utilities.Any(utility =>
                    utility.ParentOutcomeIds.Contains(stateId) ||
                    utility.ParentOptionIds.Contains(stateId)))
                .ToList();
            var addedStateIds = parentGroup.StateIds.Except(representedStateIds).ToList();

            if (representedStateIds.Count == 0)
            {
                utilities.AddUtilityParent(issueId, addedStateIds, !parentGroup.IsUncertainty);
                continue;
            }

            foreach (var addedStateId in addedStateIds)
            {
                utilities.AddUtilitiesRow(issueId, addedStateId, representedStateIds);
            }
        }
    }

    private static List<DiscreteProbabilityDto> BuildDesiredProbabilities(
        Issue issue,
        IReadOnlyCollection<ParentStateGroup> parentGroups,
        ICollection<DiscreteProbabilityDto> expandedProbabilities)
    {
        var uncertainty = issue.Uncertainty!;
        var expandedByKey = expandedProbabilities
            .GroupBy(ProbabilityKey)
            .ToDictionary(group => group.Key, group => group.First());
        var combinations = BuildCombinations(parentGroups.Select(group => group.StateIds).ToList());
        var outcomeStateIds = parentGroups
            .Where(group => group.IsUncertainty)
            .SelectMany(group => group.StateIds)
            .ToHashSet();

        return (from outcome in uncertainty.Outcomes
                from combination in combinations
                let parentOutcomeIds = combination.Where(outcomeStateIds.Contains).OrderBy(id => id).ToList()
                let parentOptionIds = combination.Where(id => !outcomeStateIds.Contains(id)).OrderBy(id => id).ToList()
                let key = ProbabilityKey(outcome.Id, parentOutcomeIds, parentOptionIds)
                let expanded = expandedByKey.GetValueOrDefault(key)
                select new DiscreteProbabilityDto
                {
                    Id = IdGenerationUtils.GetDeterministicIdProbability(issue.Id, outcome.Id, parentOutcomeIds, parentOptionIds),
                    OutcomeId = outcome.Id,
                    UncertaintyId = uncertainty.Id,
                    ProjectId = issue.ProjectId,
                    Probability = expanded?.Probability ?? 0,
                    ParentOutcomeIds = parentOutcomeIds,
                    ParentOptionIds = parentOptionIds
                }).ToList();
    }

    private static List<DiscreteUtilityDto> BuildDesiredUtilities(
        Issue issue,
        IReadOnlyCollection<ParentStateGroup> parentGroups,
        ICollection<DiscreteUtilityDto> expandedUtilities)
    {
        if (parentGroups.Count == 0)
        {
            return [];
        }

        var utility = issue.Utility!;
        var expandedByKey = expandedUtilities
            .GroupBy(UtilityKey)
            .ToDictionary(group => group.Key, group => group.First());
        var combinations = BuildCombinations(parentGroups.Select(group => group.StateIds).ToList());
        var outcomeStateIds = parentGroups
            .Where(group => group.IsUncertainty)
            .SelectMany(group => group.StateIds)
            .ToHashSet();

        // TODO: expand declarative LINQ expression to include additional utility metrics when they are added to the system
        // see BuildDesiredUtilities for an example of how to expand the LINQ expression to include additional utility metrics
        return (from combination in combinations
                let parentOutcomeIds = combination.Where(outcomeStateIds.Contains).OrderBy(id => id).ToList()
                let parentOptionIds = combination.Where(id => !outcomeStateIds.Contains(id)).OrderBy(id => id).ToList()
                let key = UtilityKey(DomainConstants.DefaultValueMetricId, parentOutcomeIds, parentOptionIds)
                let expanded = expandedByKey.GetValueOrDefault(key)
                select new DiscreteUtilityDto
                {
                    Id = IdGenerationUtils.GetDeterministicIdUtility(issue.Id, DomainConstants.DefaultValueMetricId, parentOutcomeIds, parentOptionIds),
                    ValueMetricId = DomainConstants.DefaultValueMetricId,
                    UtilityId = utility.Id,
                    ProjectId = issue.ProjectId,
                    UtilityValue = expanded?.UtilityValue ?? 0,
                    ParentOutcomeIds = parentOutcomeIds,
                    ParentOptionIds = parentOptionIds
                }).ToList();
    }

    private async Task ReconcileProbabilities(
        ICollection<DiscreteProbability> existingProbabilities,
        ICollection<DiscreteProbabilityDto> desiredProbabilities,
        CancellationToken ct)
    {
        var desiredByKey = desiredProbabilities.ToDictionary(ProbabilityKey);
        var existingGroups = existingProbabilities.GroupBy(ProbabilityKey).ToList();
        var probabilitiesToRemove = existingGroups
            .Where(group => !desiredByKey.ContainsKey(group.Key))
            .SelectMany(group => group)
            .Concat(existingGroups.SelectMany(group => group.Skip(1)))
            .Distinct()
            .ToList();
        RemoveDiscreteProbabilities(probabilitiesToRemove);

        var existingKeys = existingGroups
            .Where(group => desiredByKey.ContainsKey(group.Key))
            .Select(group => group.Key)
            .ToHashSet();
        var probabilitiesToAdd = desiredByKey
            .Where(pair => !existingKeys.Contains(pair.Key))
            .Select(pair => pair.Value.ToEntity())
            .ToList();
        await DbContext.DiscreteProbabilities.AddRangeAsync(probabilitiesToAdd, ct);
    }

    private async Task ReconcileUtilities(
        ICollection<DiscreteUtility> existingUtilities,
        ICollection<DiscreteUtilityDto> desiredUtilities,
        CancellationToken ct)
    {
        var desiredByKey = desiredUtilities.ToDictionary(UtilityKey);
        var existingGroups = existingUtilities.GroupBy(UtilityKey).ToList();
        var utilitiesToRemove = existingGroups
            .Where(group => !desiredByKey.ContainsKey(group.Key))
            .SelectMany(group => group)
            .Concat(existingGroups.SelectMany(group => group.Skip(1)))
            .Distinct()
            .ToList();
        RemoveDiscreteUtilities(utilitiesToRemove);

        var existingKeys = existingGroups
            .Where(group => desiredByKey.ContainsKey(group.Key))
            .Select(group => group.Key)
            .ToHashSet();
        var utilitiesToAdd = desiredByKey
            .Where(pair => !existingKeys.Contains(pair.Key))
            .Select(pair => pair.Value.ToEntity())
            .ToList();
        await DbContext.DiscreteUtilities.AddRangeAsync(utilitiesToAdd, ct);
    }

    private static string ProbabilityKey(DiscreteProbability probability) => ProbabilityKey(
        probability.OutcomeId,
        probability.ParentOutcomes.Select(parent => parent.ParentOutcomeId),
        probability.ParentOptions.Select(parent => parent.ParentOptionId));

    private static string ProbabilityKey(DiscreteProbabilityDto probability) => ProbabilityKey(
        probability.OutcomeId,
        probability.ParentOutcomeIds,
        probability.ParentOptionIds);

    private static string ProbabilityKey(Guid outcomeId, IEnumerable<Guid> parentOutcomeIds, IEnumerable<Guid> parentOptionIds) =>
        $"{outcomeId}|{string.Join(',', parentOutcomeIds.OrderBy(id => id))}|{string.Join(',', parentOptionIds.OrderBy(id => id))}";

    private static string UtilityKey(DiscreteUtility utility) => UtilityKey(
        utility.ValueMetricId,
        utility.ParentOutcomes.Select(parent => parent.ParentOutcomeId),
        utility.ParentOptions.Select(parent => parent.ParentOptionId));

    private static string UtilityKey(DiscreteUtilityDto utility) => UtilityKey(
        utility.ValueMetricId,
        utility.ParentOutcomeIds,
        utility.ParentOptionIds);

    private static string UtilityKey(Guid valueMetricId, IEnumerable<Guid> parentOutcomeIds, IEnumerable<Guid> parentOptionIds) =>
        $"{valueMetricId}|{string.Join(',', parentOutcomeIds.OrderBy(id => id))}|{string.Join(',', parentOptionIds.OrderBy(id => id))}";

    /// <summary>
    /// Represents a group of parent states, either from an uncertainty or a decision.
    /// </summary>
    /// <param name="StateIds"></param>
    /// <param name="IsUncertainty"></param>
    private sealed record ParentStateGroup(List<Guid> StateIds, bool IsUncertainty);

    /// <summary>
    /// Collects the parent state groups for a given issue.
    /// Each element in the list represents the states for one issue node.
    /// </summary>
    /// <param name="issue">The issue for which to collect parent state groups.</param>
    /// <returns>A list of parent state groups associated with the issue.</returns>
    private static List<ParentStateGroup> CollectParentStateGroups(Issue issue)
    {
        // Collect head edges, i.e. the edges that are pointing to the current issue node.
        // Note that parents are determined by the tail nodes of these edges.
        var edges = issue.Node?.HeadEdges ?? Array.Empty<Edge>();
        return edges
            .GroupBy(edge => new { edge.TailId, edge.HeadId })
            .Select(group => group.First().TailNode?.Issue)
            .Where(parentIssue => parentIssue != null && IsBoundaryInScope(parentIssue.Boundary))
            .Select(parentIssue =>
            {
                if (IsIssueType(parentIssue!.Type, IssueType.Uncertainty) && parentIssue.Uncertainty?.IsKey == true)
                {
                    return new ParentStateGroup(parentIssue.Uncertainty.Outcomes.Select(outcome => outcome.Id).ToList(), true);
                }

                if (IsIssueType(parentIssue.Type, IssueType.Decision) && IsDecisionFocus(parentIssue.Decision?.Type))
                {
                    return new ParentStateGroup(parentIssue.Decision!.Options.Select(option => option.Id).ToList(), false);
                }

                return null;
            })
            .Where(group => group is not null)
            .Cast<ParentStateGroup>()
            .ToList();
    }

    public async Task RemoveExcessDiscreteProbabilities(Guid uncertaintyId, CancellationToken ct = default)
    {
        var discreteProbabilities = await FindExcessDiscreteProbabilities(uncertaintyId, ct);
        RemoveDiscreteProbabilities(discreteProbabilities);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task RemoveExcessDiscreteProbabilities(CancellationToken ct = default)
    {
        var discreteProbabilities = await FindExcessDiscreteProbabilities(ct);
        RemoveDiscreteProbabilities(discreteProbabilities);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task RemoveExcessDiscreteUtilities(Guid utilityId, CancellationToken ct = default)
    {
        var discreteUtilities = await FindExcessDiscreteUtility(utilityId, ct);
        RemoveDiscreteUtilities(discreteUtilities);
        await DbContext.SaveChangesAsync(ct);
    }

    public async Task RemoveExcessDiscreteUtilities(CancellationToken ct = default)
    {
        var discreteUtilities = await FindExcessDiscreteUtility(ct);
        RemoveDiscreteUtilities(discreteUtilities);
        await DbContext.SaveChangesAsync(ct);
    }

    private async Task<IEnumerable<DiscreteProbability>> FindExcessDiscreteProbabilities(Guid uncertaintyId, CancellationToken ct = default)
    {
        var discreteProbabilities = await DbContext.DiscreteProbabilities
            .Where(p => p.UncertaintyId == uncertaintyId)
            .Include(p => p.ParentOutcomes)
            .Include(p => p.ParentOptions)
            .ToListAsync(ct);

        return discreteProbabilities
            .GroupBy(p => (
                p.OutcomeId,
                ParentOutcomes: string.Join(",", p.ParentOutcomes.Select(o => o.ParentOutcomeId).OrderBy(id => id)),
                ParentOptions: string.Join(",", p.ParentOptions.Select(o => o.ParentOptionId).OrderBy(id => id))
            ))
            .SelectMany(group => group.Skip(1));
    }

    private async Task<IEnumerable<DiscreteProbability>> FindExcessDiscreteProbabilities(CancellationToken ct = default)
    {
        var discreteProbabilities = await DbContext.DiscreteProbabilities
            .Include(p => p.ParentOutcomes)
            .Include(p => p.ParentOptions)
            .ToListAsync(ct);

        return discreteProbabilities
            .GroupBy(p => (
                p.UncertaintyId,
                p.OutcomeId,
                ParentOutcomes: string.Join(",", p.ParentOutcomes.Select(o => o.ParentOutcomeId).OrderBy(id => id)),
                ParentOptions: string.Join(",", p.ParentOptions.Select(o => o.ParentOptionId).OrderBy(id => id))
            ))
            .SelectMany(group => group.Skip(1));
    }

    private async Task<IEnumerable<DiscreteUtility>> FindExcessDiscreteUtility(Guid utilityId, CancellationToken ct = default)
    {
        var discreteUtilities = await DbContext.DiscreteUtilities
            .Where(p => p.UtilityId == utilityId)
            .Include(p => p.ParentOutcomes)
            .Include(p => p.ParentOptions)
            .ToListAsync(ct);

        return discreteUtilities
            .GroupBy(p => (
                p.ValueMetricId,
                ParentOutcomes: string.Join(",", p.ParentOutcomes.Select(o => o.ParentOutcomeId).OrderBy(id => id)),
                ParentOptions: string.Join(",", p.ParentOptions.Select(o => o.ParentOptionId).OrderBy(id => id))
            ))
            .SelectMany(group => group.Skip(1));
    }

    private async Task<IEnumerable<DiscreteUtility>> FindExcessDiscreteUtility(CancellationToken ct = default)
    {
        var discreteUtilities = await DbContext.DiscreteUtilities
            .Include(p => p.ParentOutcomes)
            .Include(p => p.ParentOptions)
            .ToListAsync(ct);

        var excessDiscreteUtilities = discreteUtilities
            .GroupBy(p => (
                p.UtilityId,
                p.ValueMetricId,
                ParentOutcomes: string.Join(",", p.ParentOutcomes.Select(o => o.ParentOutcomeId).OrderBy(id => id)),
                ParentOptions: string.Join(",", p.ParentOptions.Select(o => o.ParentOptionId).OrderBy(id => id))
            ))
            .SelectMany(group => group.Skip(1));
        return excessDiscreteUtilities;
    }

    private void RemoveDiscreteProbabilities(IEnumerable<DiscreteProbability> discreteProbabilities)
    {
        var discreteProbabilityIds = discreteProbabilities.Select(probability => probability.Id).ToList();
        if (discreteProbabilityIds.Count == 0)
        {
            return;
        }

        DbContext.DiscreteProbabilityParentOutcomes.RemoveRange(
            DbContext.DiscreteProbabilityParentOutcomes.Where(parent => discreteProbabilityIds.Contains(parent.DiscreteProbabilityId)));
        DbContext.DiscreteProbabilityParentOptions.RemoveRange(
            DbContext.DiscreteProbabilityParentOptions.Where(parent => discreteProbabilityIds.Contains(parent.DiscreteProbabilityId)));
        DbContext.DiscreteProbabilities.RemoveRange(discreteProbabilities);
    }

    private void RemoveDiscreteUtilities(IEnumerable<DiscreteUtility> discreteUtilities)
    {
        var discreteUtilityIds = discreteUtilities.Select(utility => utility.Id).ToList();
        if (discreteUtilityIds.Count == 0)
        {
            return;
        }

        DbContext.DiscreteUtilityParentOutcomes.RemoveRange(
            DbContext.DiscreteUtilityParentOutcomes.Where(parent => discreteUtilityIds.Contains(parent.DiscreteUtilityId)));
        DbContext.DiscreteUtilityParentOptions.RemoveRange(
            DbContext.DiscreteUtilityParentOptions.Where(parent => discreteUtilityIds.Contains(parent.DiscreteUtilityId)));
        DbContext.DiscreteUtilities.RemoveRange(discreteUtilities);
    }

    private static List<List<Guid>> BuildCombinations(List<List<Guid>> groups)
    {
        var results = new List<List<Guid>> { new() };
        foreach (var group in groups)
        {
            var next = new List<List<Guid>>();
            foreach (var existing in results)
            {
                foreach (var item in group)
                {
                    var combination = new List<Guid>(existing) { item };
                    next.Add(combination);
                }
            }

            results = next;
        }

        return results;
    }

    private static bool IsBoundaryInScope(string? boundary)
    {
        var normalized = Normalize(boundary);
        return normalized == Normalize(Boundary.In.ToString()) ||
               normalized == Normalize(Boundary.On.ToString());
    }

    private static bool IsIssueType(string? value, IssueType type)
    {
        return Normalize(value) == Normalize(type.ToString());
    }

    private static bool IsDecisionFocus(string? value)
    {
        return Normalize(value) == Normalize(DecisionHierarchy.Focus.ToString());
    }

    private static bool IsIssueConfiguredInInfluenceDiagram(Issue issue)
    {
        if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty != null)
        {
            return issue.Uncertainty.IsKey;
        }
        else if (IsIssueType(issue.Type, IssueType.Decision) && issue.Decision != null)
        {
            return IsDecisionFocus(issue.Decision.Type);
        }
        return false;
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
}
