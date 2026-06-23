using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Domain.Extensions;

namespace PrismaApi.Application.Services;

public class TableRebuildingService : ITableRebuildingService
{
    protected readonly AppDbContext DbContext;
    public TableRebuildingService(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    private static Guid GetDeterministicId(Guid issueId, Guid stateId, List<Guid> parentOutcomeIds, List<Guid> parentOptionIds)
    {
        // Temporary fix as original code was tested in sqlie and switching to sql server seems to have caused some issue
        return Guid.NewGuid();
        var combined = $"{issueId}|" +
                       $"StateId:{stateId}|" +
                       $"Outcomes:{string.Join(",", parentOutcomeIds.OrderBy(id => id))}|" +
                       $"Options:{string.Join(",", parentOptionIds.OrderBy(id => id))}";
        return combined.GenerateDeterministicGuid();
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
        var issueIds = DbContext.DiscreteTableSessionInfo.AffectedIssueIds;
        if (issueIds.Count != 0 && !DbContext.IsDiscreteTableEventDisabled)
        {
            await RebuildIssuesFromIssueIds(issueIds, ct);
        }
        var restrictionTableIds = DbContext.DiscreteTableSessionInfo.AffectedRestrictionTables;
        if (restrictionTableIds.Count != 0 && !DbContext.IsDiscreteTableEventDisabled)
        {
            await RebuildRestrictionTablesFromIds(restrictionTableIds, ct);
        }
        await DbContext.SaveChangesAsync(ct);
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

    private async Task RebuildUncertaintyTable(Issue issue, CancellationToken ct = default)
    {
        var uncertainty = issue.Uncertainty;
        if (uncertainty == null)
        {
            return;
        }
        RemoveDiscreteProbabilities(uncertainty.DiscreteProbabilities);
        uncertainty.DiscreteProbabilities.Clear();

        var (parentOutcomesList, parentOptionsList) = CollectParents(issue);

        if (parentOutcomesList.Count == 0 && parentOptionsList.Count == 0)
        {
            foreach (var outcome in uncertainty.Outcomes)
            {
                var newEntry = new DiscreteProbability
                {
                    Id = GetDeterministicId(
                        issue.Id, 
                        outcome.Id,
                        [.. parentOutcomesList.SelectMany(x => x)], 
                        [.. parentOptionsList.SelectMany(x => x)]
                    ),
                    OutcomeId = outcome.Id,
                    UncertaintyId = uncertainty.Id,
                    ProjectId = issue.ProjectId,
                    Probability = 0
                };
                DbContext.Entry(newEntry).State = EntityState.Added;
                await DbContext.DiscreteProbabilities.AddAsync(newEntry, ct);
            }
            return;
        }

        var parentCombinations = BuildCombinations(parentOutcomesList.Concat(parentOptionsList).ToList());
        var allOutcomes = new HashSet<Guid>(parentOutcomesList.SelectMany(list => list));
        var allOptions = new HashSet<Guid>(parentOptionsList.SelectMany(list => list));


        foreach (var outcome in uncertainty.Outcomes)
        {
            foreach (var combination in parentCombinations)
            {
                var parentOutcomeIds = combination.Where(allOutcomes.Contains).OrderBy(id => id).ToList();
                var parentOptionIds = combination.Where(allOptions.Contains).OrderBy(id => id).ToList();

                var probabilityId = GetDeterministicId(issue.Id, outcome.Id, parentOutcomeIds, parentOptionIds);

                var newEntity = new DiscreteProbability
                {
                    Id = probabilityId,
                    OutcomeId = outcome.Id,
                    UncertaintyId = uncertainty.Id,
                    ProjectId = issue.ProjectId,
                    Probability = 0,
                };
                DbContext.DiscreteProbabilities.Add(newEntity);


                var parentOptions = parentOptionIds.Select(x =>
                    new DiscreteProbabilityParentOption
                    {
                        ParentOptionId = x,
                        DiscreteProbabilityId = probabilityId,
                    }
                ).ToList();

                await DbContext.DiscreteProbabilityParentOptions.AddRangeAsync(parentOptions, ct);

                var parentOutcomes = parentOutcomeIds.Select(x =>
                    new DiscreteProbabilityParentOutcome
                    {
                        ParentOutcomeId = x,
                        DiscreteProbabilityId = probabilityId,
                    }
                ).ToList();

                await DbContext.DiscreteProbabilityParentOutcomes.AddRangeAsync(parentOutcomes, ct);
            }
        }
    }

    private async Task RebuildUtilityTable(Issue issue, CancellationToken ct = default)
    {
        var utility = issue.Utility;
        if (utility == null)
        {
            return;
        }

        var (parentOutcomesList, parentOptionsList) = CollectParents(issue);

        RemoveDiscreteUtilities(utility.DiscreteUtilities);
        utility.DiscreteUtilities.Clear();
        if (parentOutcomesList.Count == 0 && parentOptionsList.Count == 0)
        {
            return;
        }

        var parentCombinations = BuildCombinations([.. parentOutcomesList, .. parentOptionsList]);
        var allOutcomes = new HashSet<Guid>(parentOutcomesList.SelectMany(list => list));
        var allOptions = new HashSet<Guid>(parentOptionsList.SelectMany(list => list));

        var desiredKeys = new HashSet<string>();
        var desiredMap = new Dictionary<string, (List<Guid> ParentOutcomes, List<Guid> ParentOptions)>();

        foreach (var combination in parentCombinations)
        {
            var parentOutcomeIds = combination.Where(allOutcomes.Contains).OrderBy(id => id).ToList();
            var parentOptionIds = combination.Where(allOptions.Contains).OrderBy(id => id).ToList();
            var utilityId = GetDeterministicId(issue.Id, DomainConstants.DefaultValueMetricId, parentOutcomeIds, parentOptionIds);

            await DbContext.DiscreteUtilities
                .AddAsync(new DiscreteUtility
                {
                    Id = utilityId,
                    ValueMetricId = DomainConstants.DefaultValueMetricId,
                    UtilityId = utility.Id,
                    ProjectId = issue.ProjectId,
                    UtilityValue = 0,
                }, ct);


            var parentOptions = parentOptionIds.Select(x =>
                new DiscreteUtilityParentOption
                {
                    ParentOptionId = x,
                    DiscreteUtilityId = utilityId,
                }
            ).ToList();

            await DbContext.DiscreteUtilityParentOptions.AddRangeAsync(parentOptions, ct);

            var parentOutcomes = parentOutcomeIds.Select(x =>
                new DiscreteUtilityParentOutcome
                {
                    ParentOutcomeId = x,
                    DiscreteUtilityId = utilityId,
                }
            ).ToList();

            await DbContext.DiscreteUtilityParentOutcomes.AddRangeAsync(parentOutcomes, ct);
        }

    }

    private (List<List<Guid>> ParentOutcomes, List<List<Guid>> ParentOptions) CollectParents(Issue issue)
    {
        var parentOutcomesList = new List<List<Guid>>();
        var parentOptionsList = new List<List<Guid>>();

        var edges = issue.Node?.HeadEdges ?? Array.Empty<Edge>();
        var distinctEdges = edges
            .GroupBy(edge => new { edge.TailId, edge.HeadId })
            .Select(group => group.First())
            .ToList();

        foreach (var edge in distinctEdges)
        {
            var parentIssue = edge.TailNode?.Issue;
            if (parentIssue == null || !IsBoundaryInScope(parentIssue.Boundary))
            {
                continue;
            }

            if (IsIssueType(parentIssue.Type, IssueType.Uncertainty) && parentIssue.Uncertainty?.IsKey == true)
            {
                parentOutcomesList.Add(parentIssue.Uncertainty.Outcomes.Select(outcome => outcome.Id).ToList());
            }
            else if (IsIssueType(parentIssue.Type, IssueType.Decision) && IsDecisionFocus(parentIssue.Decision?.Type))
            {
                parentOptionsList.Add(parentIssue.Decision!.Options.Select(option => option.Id).ToList());
            }
        }

        return (parentOutcomesList, parentOptionsList);
    }

    public async Task RemoveExcessDataInTables(Guid projectId, UserOutgoingDto user, CancellationToken ct = default)
    {
        var discreteUncertaintyIds = await DbContext.Uncertainties
            .Where(x => x.Issue!.ProjectId == projectId && x.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id))
            .Select(e => e.Id).ToListAsync(ct);
        var discreteUtilityIds = await DbContext.Utilities
            .Where(x => x.Issue!.ProjectId == projectId && x.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id))
            .Select(e => e.Id).ToListAsync(ct);
        foreach (var id in discreteUncertaintyIds) await RemoveExcessDiscreteProbabilities(id, ct);
        foreach (var id in discreteUtilityIds) await RemoveExcessDiscreteUtilities(id, ct);
        await DbContext.SaveChangesAsync(ct);
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

        return discreteUtilities
            .GroupBy(p => (
                p.ValueMetricId,
                ParentOutcomes: string.Join(",", p.ParentOutcomes.Select(o => o.ParentOutcomeId).OrderBy(id => id)),
                ParentOptions: string.Join(",", p.ParentOptions.Select(o => o.ParentOptionId).OrderBy(id => id))
            ))
            .SelectMany(group => group.Skip(1));
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
