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
    
    public static void ApplyRestrictions(this InfluenceDiagramDto influenceDiagramDto)
    {
        var restrictionEntries = influenceDiagramDto.restrictionTables.SelectMany(rt => rt.RestrictionEntries).ToList();
        var discreteProbabilities = influenceDiagramDto.discreteProbabilities;

        foreach (var entry in restrictionEntries)
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
            else
            {
                if (entry.RestrictionValue != 0)
                {
                    // only need to set the utility to -inf if the restriction value is 0, otherwise we can leave it as is
                    continue; 
                }
                var affectedOptions = influenceDiagramDto.issues
                    .Where(x => x.Type == IssueType.Decision.ToString() && x.Decision is not null && x.Decision.Options.Count != 0)
                    .SelectMany(y => y.Decision!.Options)
                    .Where(op => op.Id == entry.ChildStateId);

                foreach (var option in affectedOptions)
                {
                    option.Utility = double.MinValue;
                    // option.Utility = double.NegativeInfinity;
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