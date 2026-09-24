using System.Text.Json;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaDotnetApi.PrismaApi.Domain.Extensions;

namespace PrismaApi.Domain.Extensions;

public static class InfluenceDiagramDtoExtensions
{
    public static InfluenceDiagramDto DeepClone<InfluenceDiagramDto>(this InfluenceDiagramDto source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<InfluenceDiagramDto>(json)!;
    }

    private static IssueOutgoingDto AddUtilityIssue(
        this InfluenceDiagramDto influenceDiagramDto,
        Guid utilityId,
        string name,
        params NodeOutgoingDto[] parentNodes)
    {
        var utilityNode = new NodeViaIssueOutgoingDto
        {
            Id = utilityId,
            IssueId = utilityId,
            ProjectId = influenceDiagramDto.projectId,
            Name = name,
        };
        var utilityIssue = new IssueOutgoingDto
        {
            Id = utilityId,
            ProjectId = influenceDiagramDto.projectId,
            Name = name,
            Type = IssueType.Utility.ToString(),
            Utility = new UtilityOutgoingDto
            {
                ProjectId = influenceDiagramDto.projectId,
                Id = utilityId,
                IssueId = utilityId,
            },
            Node = utilityNode
        };
        var utilityNodeWithIssue = new NodeOutgoingDto
        {
            Id = utilityId,
            IssueId = utilityId,
            ProjectId = influenceDiagramDto.projectId,
            Name = name,
            Issue = new IssueViaNodeOutgoingDto
            {
                Id = utilityId,
                ProjectId = influenceDiagramDto.projectId,
                Name = name,
                Type = IssueType.Utility.ToString(),
                Utility = utilityIssue.Utility,
            }
        };

        influenceDiagramDto.issues.Add(utilityIssue);
        foreach (var parentNode in parentNodes)
        {
            influenceDiagramDto.edges.Add(new EdgeOutgoingDto
            {
                Id = Guid.NewGuid(),
                ProjectId = influenceDiagramDto.projectId,
                TailIssueId = parentNode.IssueId,
                TailId = parentNode.Id,
                TailNode = parentNode,
                HeadIssueId = utilityId,
                HeadId = utilityId,
                HeadNode = utilityNodeWithIssue
            });
        }

        return utilityIssue;
    }

    private static void CreateRestrictedDiscreteUtilities(this InfluenceDiagramDto influenceDiagramDto, Guid restrictionTableId, EdgeOutgoingDto edge, string name)
    {
        var restrictionTable = influenceDiagramDto.restrictionTables.FirstOrDefault(rt => rt.Id == restrictionTableId);
        if (restrictionTable is null) return;

        influenceDiagramDto.AddUtilityIssue(restrictionTableId, name, edge.TailNode, edge.HeadNode);
        foreach (var entry in restrictionTable.RestrictionEntries)
        {
            if (!entry.IsChildUncertainty && entry.ParentStateId is not null && entry.ChildStateId is not null)
            {
                // The child of the entry is an option, meaning it is a parent of the discrete utility, but the parent can be either an option or an outcome, 
                // Check which one it is and add it to the appropriate list of parent ids
                var parentOutcomeIds = entry.IsParentUncertainty ? new List<Guid> { (Guid)entry.ParentStateId } : new List<Guid>();
                var parentOptionIds = entry.IsParentUncertainty ? new List<Guid>{(Guid)entry.ChildStateId} : new List<Guid> { (Guid)entry.ChildStateId, (Guid)entry.ParentStateId };
                // create a discrete utility for the restricted option given the parent state
                var discreteUtility = new DiscreteUtilityDto
                {
                    ProjectId = influenceDiagramDto.projectId,
                    UtilityId = restrictionTableId,
                    ParentOptionIds = parentOptionIds,
                    ParentOutcomeIds = parentOutcomeIds,
                    // using -1e100 instead of double.MinValue because the solver cannot handle extremely large negative values
                    UtilityValue = entry.RestrictionValue == 0 ? -1e100 : 0 
                };
                influenceDiagramDto.discreteUtilities.Add(discreteUtility);
            }
        }
    }
    
    public static void ApplyRestrictions(this InfluenceDiagramDto influenceDiagramDto)
    {
        influenceDiagramDto.ValidateRestrictions();
        RestrictDecisions(influenceDiagramDto);
        RestrictUncertainties(influenceDiagramDto);
    }

    public static void ValidateRestrictions(this InfluenceDiagramDto influenceDiagramDto)
    {
        var tablesWithTotalRestrictions = influenceDiagramDto.restrictionTables
            .Where(table => table.RestrictionEntries
                .Where(entry => entry.ParentStateId is not null)
                .GroupBy(entry => entry.ParentStateId)
                .Any(row => row.All(entry => entry.RestrictionValue == 0)));

        var utilityIssueIds = influenceDiagramDto.issues
            .Where(issue => issue.Type == IssueType.Utility.ToString())
            .Select(issue => issue.Id)
            .ToList();

        foreach (var table in tablesWithTotalRestrictions)
        {
            // TODO: Dont requre edge to utilities
            // TODO: the head of the total restriction cannot be the parent of an uncertainty
            var restrictedEdge = influenceDiagramDto.edges.FirstOrDefault(edge => edge.Id == table.EdgeId)
                ?? throw new InvalidOperationException($"Restriction table '{table.Id}' references an edge that is not in the influence diagram.");
            
            var childIssueIds = influenceDiagramDto.edges
                .Where(edge => edge.TailIssueId == restrictedEdge.HeadIssueId && !utilityIssueIds.Contains(edge.HeadIssueId)) // filter out utility issues as children we don't have to consider
                .Select(edge => edge.HeadIssueId)
                .Distinct();

            // if any of the children are of type uncertainty => throw invalid exception with details of the issue:

            if (childIssueIds.Any(childIssueId => influenceDiagramDto.issues.FirstOrDefault(issue => issue.Id == childIssueId)?.Type == IssueType.Uncertainty.ToString()))
            {
                throw new InvalidOperationException(
                    $"Restriction table '{table.Id}' totally restricts an available row, but node '{restrictedEdge.HeadIssueId}' has a child of type uncertainty. This does not provide the nessessary information for total restriction. To address this try reversing the order of Uncertainties in the influence diagram if possible.");
            }
            
            var missingChildIssueIds = childIssueIds
                .Where(childIssueId => !influenceDiagramDto.edges.Any(edge =>
                    edge.TailIssueId == restrictedEdge.TailIssueId && edge.HeadIssueId == childIssueId))
                .ToList();

            if (missingChildIssueIds.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Restriction table '{table.Id}' totally restricts an available row, but node '{restrictedEdge.TailIssueId}' does not have an edge to every child of node '{restrictedEdge.HeadIssueId}'. Missing child node ids: {string.Join(", ", missingChildIssueIds)}.");
            }
        }
    }

    public static void ApplyTotalRestrictions(this InfluenceDiagramDto influenceDiagramDto)
    {
        influenceDiagramDto.ValidateRestrictions();
        // this is performed after partial restrictions
        // get issue ids in topological order to apply the total restrictions in an orderly manner
        var orderedIssueIds = influenceDiagramDto.OrderIssueIdsByTopologicalSort();
        foreach (var issueId in orderedIssueIds)
        {
            var issue = influenceDiagramDto.issues.FirstOrDefault(i => i.Id == issueId)
                ?? throw new InvalidOperationException($"Issue with id '{issueId}' not found in the influence diagram.");
            // apply total restrictions for the issue with id 'issueId'
            // 1) check if total restriction applies to this issue
            // meaning that for the first issue there is no restriction table
            var restrictionTablesForIssue = influenceDiagramDto.restrictionTables
                .Where(table => table.EdgeId == influenceDiagramDto.edges.FirstOrDefault(edge => edge.HeadIssueId == issueId)?.Id)
                .ToList();
            if (!restrictionTablesForIssue.Any())
            {
                continue; // no restrictions for this issue
            }
            // throw exception if all entries are restricted
            if (restrictionTablesForIssue.All(table => table.RestrictionEntries.All(entry => entry.RestrictionValue == 1)))
            {
                throw new InvalidOperationException($"All entries for issue with id '{issueId}' are restricted.");
            }

            // need to handle on a row basis
            var restrictionEntriesByRow = restrictionTablesForIssue
                .SelectMany(table => table.RestrictionEntries)
                .GroupBy(entry => entry.ParentStateId)
                .ToList();

            foreach (var row in restrictionEntriesByRow)
            {
                if (!row.All(entry => entry.RestrictionValue == 1))
                {
                    continue; // skip this row if not all entries are restricted
                }
                var parentStateId = row.Key;
                if (parentStateId == Guid.Empty || parentStateId == null)
                {
                    continue; // skip if parent state id is not valid
                }
                // apply total restriction logic for this issue here
                if (issue.Type == IssueType.Decision.ToString() && issue.Decision is not null)
                {
                    var notApplicableOption = new OptionOutgoingDto{ProjectId = issue.ProjectId, Id = Guid.NewGuid(), Name = "N/A" };
                    issue.Decision.Options.Add(notApplicableOption);
                    // all children that are utilities need new discreteutilities
                    foreach (var childEdge in influenceDiagramDto.edges.Where(edge => edge.TailIssueId == issueId))
                    {
                        var childIssue = influenceDiagramDto.issues.FirstOrDefault(i => i.Id == childEdge.HeadIssueId);
                        if (childIssue?.Type == IssueType.Utility.ToString() && childIssue.Utility is not null)
                        {
                            influenceDiagramDto.AddUtilitiesFromNAState(childIssue, notApplicableOption, issue.Decision);
                        }
                    }
                }
                if (issue.Type == IssueType.Uncertainty.ToString() && issue.Uncertainty is not null)
                {
                    var notApplicableOutcome = new OutcomeOutgoingDto{ProjectId = issue.ProjectId, Id = Guid.NewGuid(), Name = "N/A" };
                    issue.Uncertainty.Outcomes.Add(notApplicableOutcome);
                    // all children that are utilities need new discreteutilities
                    foreach (var childEdge in influenceDiagramDto.edges.Where(edge => edge.TailIssueId == issueId))
                    {
                        var childIssue = influenceDiagramDto.issues.FirstOrDefault(i => i.Id == childEdge.HeadIssueId);
                        if (childIssue?.Type == IssueType.Utility.ToString() && childIssue.Utility is not null)
                        {
                            influenceDiagramDto.AddUtilitiesFromNAState(childIssue, notApplicableOutcome, issue.Uncertainty);
                            // need to add the new outcome as column in it's probability table
                            influenceDiagramDto.AddProbabilitiesFromAddedOutcome((Guid)parentStateId, notApplicableOutcome, issue.Uncertainty);
                        }
                    }
                }
            }

            
        }
    }

    public static void AddUtilitiesFromNAState(this InfluenceDiagramDto influenceDiagramDto, IssueOutgoingDto utilityIssue, OptionOutgoingDto notApplicableState, DecisionOutgoingDto parent)
    {
        // there are exsiting combinations of parent states that need to be considered when adding utilities from the N/A state
        // example: parent utilities => a1 and b1, from Decision A and Uncertainty B respectively
        // adding N/A to Decision A implies adding rows for N/A, b1 and N/A, b2 ... N/A, bn for all existing combinations of parent states
        // default utility of these new utilities to 0
        if (utilityIssue.Utility is null)
        {
            throw new InvalidOperationException("Utility issue does not have an associated utility.");
        }
        // use one of the existing parent states as a template for creating new utilities for the N/A state
        var existingParentState = parent.Options
            .FirstOrDefault(option => option.Id != notApplicableState.Id)
            ?? throw new InvalidOperationException("The parent decision does not have an existing option.");

        var existingUtilities = influenceDiagramDto.discreteUtilities
            .Where(utility => utility.UtilityId == utilityIssue.Utility.Id)
            .ToList();

        var utilitiesForExistingParentState = existingUtilities
            .Where(utility => utility.ParentOptionIds.Contains(existingParentState.Id))
            .ToList();

        var newUtilities = utilitiesForExistingParentState
            .Select(utility => new DiscreteUtilityDto
            {
                ProjectId = utility.ProjectId,
                UtilityId = utility.UtilityId,
                ValueMetricId = utility.ValueMetricId,
                UtilityValue = 0,
                ParentOptionIds = utility.ParentOptionIds
                    .Select(id => id == existingParentState.Id ? notApplicableState.Id : id)
                    .ToList(),
                ParentOutcomeIds = [.. utility.ParentOutcomeIds]
            })
            .ToList();

        foreach (var newUtility in newUtilities)
        {
            influenceDiagramDto.discreteUtilities.Add(newUtility);
        }
    }

    public static void AddUtilitiesFromNAState(this InfluenceDiagramDto influenceDiagramDto, IssueOutgoingDto utilityIssue, OutcomeOutgoingDto notApplicableState, UncertaintyOutgoingDto parent)
    {
        if (utilityIssue.Utility is null)
        {
            throw new InvalidOperationException("Utility issue does not have an associated utility.");
        }
        // use one of the existing parent states as a template for creating new utilities for the N/A state
        var existingParentState = parent.Outcomes
            .FirstOrDefault(outcome => outcome.Id != notApplicableState.Id)
            ?? throw new InvalidOperationException("The parent decision does not have an existing option.");

        var existingUtilities = influenceDiagramDto.discreteUtilities
            .Where(utility => utility.UtilityId == utilityIssue.Utility.Id)
            .ToList();
        
        var newUtilities = existingUtilities
            .Where(utility => utility.ParentOutcomeIds.Contains(existingParentState.Id))
            .Select(utility => new DiscreteUtilityDto
            {
                ProjectId = utility.ProjectId,
                UtilityId = utility.UtilityId,
                ValueMetricId = utility.ValueMetricId,
                UtilityValue = 0,
                ParentOutcomeIds = utility.ParentOutcomeIds
                    .Select(id => id == existingParentState.Id ? notApplicableState.Id : id)
                    .ToList(),
                ParentOptionIds = [.. utility.ParentOptionIds]
            })
            .ToList();

        foreach (var newUtility in newUtilities)
        {
            influenceDiagramDto.discreteUtilities.Add(newUtility);
        }
    }

    private static void AddProbabilitiesFromAddedOutcome(this InfluenceDiagramDto influenceDiagramDto, Guid restrictionParentStateId, OutcomeOutgoingDto addedOutcome, UncertaintyOutgoingDto parent)
    {
        // this is an added outcome that must add corresponding probabilities for the new outcome in the parent uncertainty's probability table.
        // for the existing rows when this outcome is added it's probability will be 0. 
        // then for new rows added the probability for this new outcome will be 1 for the N/A state and 0 for all other outcomes.

        var relevantDiscreteProbabilities = influenceDiagramDto.discreteProbabilities
            .Where(probability => probability.UncertaintyId == parent.Id)
            .ToList();
        relevantDiscreteProbabilities.AddProbabilitiesColumn(addedOutcome);
        relevantDiscreteProbabilities.SetProbabilitiesToOne(restrictionParentStateId, addedOutcome.Id);

        // from influence diagram replace all probabilities with the uncertainty id with the updated relevantDiscreteProbabilities
        foreach (var probability in relevantDiscreteProbabilities)
        {
            influenceDiagramDto.discreteProbabilities.Remove(probability);
            influenceDiagramDto.discreteProbabilities.Add(probability);
        }

    }

    public static IEnumerable<Guid> OrderIssueIdsByTopologicalSort(this InfluenceDiagramDto influenceDiagramDto)
    {
        var sorted = new List<Guid>();
        var visited = new HashSet<Guid>();

        void Visit(Guid issueId)
        {
            if (!visited.Contains(issueId))
            {
                visited.Add(issueId);
                var children = influenceDiagramDto.edges
                    .Where(edge => edge.TailIssueId == issueId)
                    .Select(edge => edge.HeadIssueId);
                foreach (var child in children)
                {
                    Visit(child);
                }
                sorted.Add(issueId);
            }
        }

        var allIssueIds = influenceDiagramDto.edges
            .SelectMany(edge => new[] { edge.TailIssueId, edge.HeadIssueId })
            .Distinct();

        foreach (var issueId in allIssueIds)
        {
            Visit(issueId);
        }

        sorted.Reverse();
        return sorted;
    }

    public static bool RequiresMarginsForRestrictions(this InfluenceDiagramDto influenceDiagramDto)
    {
        // Check if there are any total restrictions (i.e., restrictions where all entries for a given parent state have a restriction value of 0)
        // this could require margins for rebalincing the probabilities
        return influenceDiagramDto.restrictionTables.Any(restrictionTable =>
            restrictionTable.RestrictionEntries
                .Where(entry => entry.ParentStateId is not null)
                .GroupBy(entry => entry.ParentStateId)
                .Any(row => row.All(entry => entry.RestrictionValue == 0)));
    }

    private static void RestrictDecisions(InfluenceDiagramDto influenceDiagramDto)
    {
        // Get all restriction tables that apply to decisions and have at least one restriction entry with a value other than 1
        var restrictionTablesDecisions = influenceDiagramDto.restrictionTables
            .Where(rt => !rt.RestrictionEntries.All(re => re.IsChildUncertainty) &&
                rt.RestrictionEntries.Any(re => re.RestrictionValue != 1) // only apply restrictions if there are any entries with a restriction value other than 1
            )
            .ToList();
        var i = 0;
        foreach (var table in restrictionTablesDecisions)
        {
            // skip if all entries have a restriction value of 1, meaning no restrictions
            if (table.RestrictionEntries.All(re => re.RestrictionValue == 1)) continue;
            var edge = influenceDiagramDto.edges.First(e => e.Id == table.EdgeId);
            influenceDiagramDto.CreateRestrictedDiscreteUtilities(table.Id, edge, $"Restricted utility {i}");
            i++;
        }
    }

    private static void RestrictUncertainties(InfluenceDiagramDto influenceDiagramDto)
    {
        var discreteProbabilities = influenceDiagramDto.discreteProbabilities;
        // Get all restriction entries that apply to uncertainties and have a restriction value other than 1
        var restrictionEntriesUncertainties = influenceDiagramDto.restrictionTables
            .SelectMany(rt => rt.RestrictionEntries)
            .Where(re => re.IsChildUncertainty && re.RestrictionValue != 1) // only apply restrictions if there are any entries with a restriction value other than 1
            .ToList();
        foreach (var entry in restrictionEntriesUncertainties)
        {
            if (entry.IsChildUncertainty)
            {
                // child is an outcome, set the discrete probabilities that have that outcome/option as a parent to 0
                var affectedProbabilities = discreteProbabilities.Where(
                    dp => dp.OutcomeId == entry.ChildStateId &&
                    entry.ParentStateId is not null &&
                    (dp.ParentOptionIds.Contains((Guid)entry.ParentStateId) || dp.ParentOutcomeIds.Contains((Guid)entry.ParentStateId))).ToList();
                foreach (var probability in affectedProbabilities)
                {
                    probability.Probability = probability.Probability * entry.RestrictionValue;
                    // need to normalize the probabilities for the parent state after setting some to 0
                    // solver hanldes the case where all probabilities for a parent state are set to 0
                }
            }
        }
        discreteProbabilities.NormalizeProbabilities();
    }
}