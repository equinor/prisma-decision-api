using System.Text.Json;
using PrismaApi.Domain.Constants;

namespace PrismaApi.Domain.Dtos;

public class InfluenceDiagramDto
{
    public required Guid projectId { get; init; }
    public required ICollection<IssueOutgoingDto> issues { get; init; }
    public required ICollection<EdgeOutgoingDto> edges { get; init; }
    public required ICollection<DiscreteProbabilityDto> discreteProbabilities { get; init; }
    public required ICollection<DiscreteUtilityDto> discreteUtilities { get; init; }
    public required ICollection<RestrictionTableOutgoingDto> restrictionTables { get; init; }
}

public static class InfluenceDiagramDtoExtensions
{
    public static InfluenceDiagramDto DeepClone<InfluenceDiagramDto>(this InfluenceDiagramDto source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<InfluenceDiagramDto>(json)!;
    }

    public static List<DiscreteUtilityDto> GetRestrictedDiscreteUtilities(this InfluenceDiagramDto influanceDiagramDto, Guid RestrictionTableId)
    {
        return influanceDiagramDto.discreteUtilities.Where(du => du.UtilityId == RestrictionTableId).ToList();
    }

    public static void CreateRestrictedDiscreteUtilities(this InfluenceDiagramDto influanceDiagramDto, Guid RestrictionTableId, EdgeOutgoingDto edge)
    {
        var restrictionTable = influanceDiagramDto.restrictionTables.FirstOrDefault(rt => rt.Id == RestrictionTableId);
        if (restrictionTable is null) return;

        // create the utility issue and nodes and edges
        var utilityNode = new NodeViaIssueOutgoingDto
        {
            Id = RestrictionTableId,
            IssueId = RestrictionTableId,
            Name = restrictionTable.Name,
        };
        var utilityIssue = new IssueOutgoingDto
        {
            Id = RestrictionTableId,
            Name = restrictionTable.Name,
            Type = IssueType.Utility.ToString(),
            Utility = new UtilityOutgoingDto
            {
                ProjectId = influanceDiagramDto.projectId,
                Id = RestrictionTableId,
            },
            Node = utilityNode
        };
        var nodeDto = new NodeOutgoingDto
        {
            Id = RestrictionTableId,
            IssueId = RestrictionTableId,
            Name = restrictionTable.Name,
            Issue = new IssueViaNodeOutgoingDto
            {
                Id = RestrictionTableId,
                Name = restrictionTable.Name,
                Type = IssueType.Utility.ToString(),
                Utility = new UtilityOutgoingDto
                {
                    ProjectId = influanceDiagramDto.projectId,
                    Id = RestrictionTableId,
                },
            }
        };

        var edge1 = new EdgeOutgoingDto
        {
            Id = Guid.NewGuid(),
            TailIssueId = influanceDiagramDto.issues.First(i => i.Node.Id == edge.TailId).Id,
            TailId = edge.TailId,
            TailNode = edge.TailNode,
            HeadIssueId = utilityNode.IssueId,
            HeadId = utilityNode.Id,
            HeadNode = nodeDto
        };
        var edge2 = new EdgeOutgoingDto
        {
            Id = Guid.NewGuid(),
            TailIssueId = influanceDiagramDto.issues.First(i => i.Node.Id == edge.HeadId).Id,
            TailId = edge.HeadId,
            TailNode = edge.HeadNode,
            HeadIssueId = utilityNode.IssueId,
            HeadId = utilityNode.Id,
            HeadNode = nodeDto
        };
        influanceDiagramDto.issues.Add(utilityIssue);
        influanceDiagramDto.edges.Add(edge1);
        influanceDiagramDto.edges.Add(edge2);
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
                    ProjectId = influanceDiagramDto.projectId,
                    UtilityId = RestrictionTableId,
                    ParentOptionIds = parentOptionIds,
                    ParentOutcomeIds = parentOutcomeIds,
                    UtilityValue = entry.RestrictionValue == 0 ? double.MinValue : 0
                };
                influanceDiagramDto.discreteUtilities.Add(discreteUtility);
            }
        }
    }
    
    public static void ApplyRestrictions(this InfluenceDiagramDto influenceDiagramDto)
    {
        // var restrictionEntries = influenceDiagramDto.restrictionTables.SelectMany(rt => rt.RestrictionEntries).ToList();
        // we can apply uncertainty restrictions to the existing discrete probabilities, 
        // but for decision restrictions we need to create new discrete utilities for the restricted options given the parent states
        var restrictionEntriesUncertainties = influenceDiagramDto.restrictionTables.SelectMany(rt => rt.RestrictionEntries).Where(re => re.IsChildUncertainty).ToList();
        var restrictionTablesDecisions = influenceDiagramDto.restrictionTables.Where(rt => !rt.RestrictionEntries.All(re => re.IsChildUncertainty)).ToList();
        var discreteProbabilities = influenceDiagramDto.discreteProbabilities;

        foreach (var table in restrictionTablesDecisions)
        {
            // skip if all entries have a restriction value of 1, meaning no restrictions
            if (table.RestrictionEntries.All(re => re.RestrictionValue == 1)) continue;
            var edge = influenceDiagramDto.edges.First(e => e.Id == table.EdgeId);
            influenceDiagramDto.CreateRestrictedDiscreteUtilities(table.Id, edge);
        }
        foreach (var entry in restrictionEntriesUncertainties)
        {
            if (entry.IsChildUncertainty)
            {
                // child is an outcome, set the discrete probabilities that have that outcome as a parent to 0
                var affectedProbabilities = discreteProbabilities.Where(
                    dp => dp.OutcomeId == entry.ChildStateId && 
                    entry.ParentStateId is not null && 
                    (dp.ParentOptionIds.Contains((Guid)entry.ParentStateId) || dp.ParentOutcomeIds.Contains((Guid)entry.ParentStateId))).ToList();
                foreach (var probability in affectedProbabilities)
                {
                    probability.Probability = probability.Probability * entry.RestrictionValue;
                    // need to normalize the probabilities for the parent state after setting some to 0
                    // need to also address the case where all probabilities for a parent state are set to 0, in which case we need to eliminate that parent state? Edit: pyagrum handles this, but does not normalize the probabilities, so we need to normalize them ourselves
                }
            }
        }
        var probabilityRows = discreteProbabilities.GroupBy(dp =>
            string.Join(",", dp.ParentOptionIds.OrderBy(x => x)
                .Concat(dp.ParentOutcomeIds.OrderBy(x => x)))).ToList();
        foreach (var row in probabilityRows)
        {
            int precision = 2;
            var totalProbability = row.Sum(x => x.Probability);
            if (totalProbability is null || Math.Round(totalProbability.Value, precision) == 0 || Math.Round(totalProbability.Value, precision) == 1) continue; // no need to normalize 
            
            // normalize the probabilities for this row, but leave out any probabilities that are already 0, since they are restricted and should not be normalized
            var nonZeroProbabilities = row.Where(x => x.Probability > 0).ToList();
            var nonZeroTotalProbability = nonZeroProbabilities.Sum(x => x.Probability);
            foreach (var probability in nonZeroProbabilities)
            {
                probability.Probability = probability.Probability / nonZeroTotalProbability;
            }
        }
    }
}