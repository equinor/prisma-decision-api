using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Utilities;

namespace PrismaDotnetApi.PrismaApi.Domain.Extensions;

public static class DiscreteProbabilitiesExtensions
{
    public static void AddProbabilitiesColumn(this ICollection<DiscreteProbabilityDto> probabilities, OutcomeOutgoingDto addedOutcome)
    {
        probabilities.ValidateProbabilities();
        // Implementation for adding a probabilities column goes here
        var rows = probabilities.SeperateByRow();
        // each row needs a new entry for the added outcome, defaults to 0 probability
        foreach (var row in rows)
        {
            foreach (var dp in row)
            {
                probabilities.Add(new DiscreteProbabilityDto
                {
                    Id = Utilities.GetDeterministicId(dp.ProjectId, addedOutcome.Id, dp.ParentOutcomeIds, dp.ParentOptionIds),
                    ProjectId = dp.ProjectId,
                    UncertaintyId = dp.UncertaintyId,
                    ParentOptionIds = new List<Guid>(dp.ParentOptionIds),
                    ParentOutcomeIds = new List<Guid>(dp.ParentOutcomeIds),
                    OutcomeId = addedOutcome.Id,
                    Probability = 0
                });
            }
        }
    }

    public static void SetProbabilitiesToOne(this ICollection<DiscreteProbabilityDto> probabilities, Guid parentStateId, Guid outcomeId)
    {
        probabilities.ValidateProbabilities();
        var rows = probabilities.SeperateByRow();
        // for each row where parent state is in parent option ids or parent outcome ids, set the probability of the specified outcome to 1 and the other outcomes to 0
        foreach (var row in rows)
        {
            if (row.Any(dp => dp.ParentOptionIds.Contains(parentStateId) || dp.ParentOutcomeIds.Contains(parentStateId)))
            {
                foreach (var dp in row)
                {
                    if (dp.OutcomeId == outcomeId)
                    {
                        dp.Probability = 1;
                    }
                    else
                    {
                        dp.Probability = 0;
                    }
                }
            }
        }   
    }

    public static void AddProbabilitiesRow(this ICollection<DiscreteProbabilityDto> probabilities, Guid addedStateId, List<Guid> siblingsOfAddedState)
    {
        probabilities.ValidateProbabilities();
        // Implementation for adding a probabilities row goes here
        var rows = probabilities.SeperateByRow();
        // add rows for the added state, the siblings already exist in the current rows and one sibling can be used as a template for the new rows
        // for the example sibling, take all rows where that sibling is a parent outcome/option and duplicate them, but replace the sibling id with the added state id, default to 0 probability
        var exampleSiblingId = siblingsOfAddedState.First();
        var rowsToDuplicate = rows.Where(row => row.Any(dp => dp.ParentOptionIds.Contains(exampleSiblingId) || dp.ParentOutcomeIds.Contains(exampleSiblingId))).ToList();
        foreach (var row in rowsToDuplicate)
        {
            foreach (var dp in row)
            {
                var newDp = new DiscreteProbabilityDto
                {
                    Id = Utilities.GetDeterministicId(dp.ProjectId, addedStateId, dp.ParentOutcomeIds, dp.ParentOptionIds),
                    ProjectId = dp.ProjectId,
                    UncertaintyId = dp.UncertaintyId,
                    ParentOptionIds = new List<Guid>(dp.ParentOptionIds),
                    ParentOutcomeIds = new List<Guid>(dp.ParentOutcomeIds),
                    OutcomeId = dp.OutcomeId,
                    Probability = 0
                };
                if (newDp.ParentOptionIds.Remove(exampleSiblingId))
                {
                    newDp.ParentOptionIds.Add(addedStateId);
                }
                else if (newDp.ParentOutcomeIds.Remove(exampleSiblingId))
                {
                    newDp.ParentOutcomeIds.Add(addedStateId);
                }
                probabilities.Add(newDp);
            }
        }

    }

    private static List<IGrouping<string, DiscreteProbabilityDto>> SeperateByRow(this ICollection<DiscreteProbabilityDto> probabilities)
    {
        // a row shares all parent option ids and all parent outcome ids
        var rowsGrouping = probabilities.GroupBy(dp =>
            string.Join(",", dp.ParentOptionIds.OrderBy(x => x)
                .Concat(dp.ParentOutcomeIds.OrderBy(x => x))))
            .ToList();
        return rowsGrouping;
    }

    private static void ValidateProbabilities(this ICollection<DiscreteProbabilityDto> probabilities)
    {
        if (probabilities.Count == 0)
        {
            throw new InvalidOperationException("No probabilities provided.");
        }
        probabilities.ValidateAllProbabilitiesBelongToSameUncertainty();
    }

    private static void ValidateAllProbabilitiesBelongToSameUncertainty(this ICollection<DiscreteProbabilityDto> probabilities)
    {
        if (!probabilities.Any())
        {
            return;
        }

        var firstUncertaintyId = probabilities.First().UncertaintyId;
        if (probabilities.Any(p => p.UncertaintyId != firstUncertaintyId))
        {
            throw new InvalidOperationException("All probabilities must belong to the same uncertainty.");
        }
    }

    public static void NormalizeProbabilities(this ICollection<DiscreteProbabilityDto> probabilities)
    {
        var probabilityRows = probabilities.SeperateByRow();
        foreach (var row in probabilityRows)
        {
            int precision = 2;
            var totalProbability = row.Sum(x => x.Probability);
            if (totalProbability is null || Math.Round(totalProbability.Value, precision) == 0 || Math.Round(totalProbability.Value, precision) == 1) continue; // no need to normalize 

            // normalize the probabilities for this row, but leave out any probabilities that are already 0
            // since they are restricted and should not be normalized
            var nonZeroProbabilities = row.Where(x => x.Probability > 0).ToList();
            var nonZeroTotalProbability = nonZeroProbabilities.Sum(x => x.Probability);
            foreach (var probability in nonZeroProbabilities)
            {
                probability.Probability = probability.Probability / nonZeroTotalProbability;
            }
        }
    }
}
