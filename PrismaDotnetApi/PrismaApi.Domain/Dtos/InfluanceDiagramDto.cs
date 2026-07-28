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


// add extension method to replace the restrictionservice.ApplyRestrictions method with a more functional approach

public static class InfluenceDiagramDtoExtensions
{
    public static void ApplyRestrictions(this InfluenceDiagramDto influenceDiagramDto)
    {
        var restrictionTables = influenceDiagramDto.restrictionTables;
        var restrictionEntries = restrictionTables.SelectMany(rt => rt.RestrictionEntries).ToList();
        var discreteProbabilities = influenceDiagramDto.discreteProbabilities;
        var discreteUtilities = influenceDiagramDto.discreteUtilities;

        foreach (var entry in restrictionEntries)
        {
            if (entry.IsChildUncertainty)
            {
                // child is an outcome, set the discrete probabilities that have that outcome as a parent to 0
                var affectedProbabilities = discreteProbabilities.Where(dp => dp.OutcomeId == entry.ChildStateId);
                foreach (var probability in affectedProbabilities)
                {
                    probability.Probability = 0;
                    // need to normalize the probabilities for the parent state after setting some to 0
                    // need to also address the case where all probabilities for a parent state are set to 0, in which case we need to eliminate that parent state? unsure if pyagrum can handle that on the backend, so assume that if all probabilities for a row are 0 skip normalization
                }
            }
            else
            {
                var affectedOptions = influenceDiagramDto.issues
                    .Where(x => x.Type == IssueType.Decision.ToString() && x.Decision is not null && x.Decision.Options.Count != 0)
                    .SelectMany(y => y.Decision!.Options)
                    .Where(op => op.Id == entry.ChildStateId);

                foreach (var option in affectedOptions)
                {
                    option.Utility = double.NegativeInfinity;
                }
            }
        }
    }
}