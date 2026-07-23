using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class BayesService
{

    public BayesService()
    {
    }

    public async Task<List<DiscreteProbabilityDto>> ReverseConditionalProbabilityAsync(List<UncertaintyOutgoingDto> probabilities, Guid newTailId, Guid newHeadId, CancellationToken ct = default)
    {
        // get all uncertainties that are in the influance diagram with outcomes and probabilities
        // assumes that the tail and head are probabilities

        // implement helper functions to calculate the needed probabilities as needed, for example, P(A|B) = P(B|A) * P(A) / P(B)
        // or the P(a1) = P(a1|b1) * P(b1) + P(a1|b2) * P(b2) + ... + P(a1|bn) * P(bn)

        var newTailUncertainty = probabilities.FirstOrDefault(p => p.Id == newTailId);
        var newHeadUncertainty = probabilities.FirstOrDefault(p => p.Id == newHeadId);
        var newTailOutcomes = newTailUncertainty!.Outcomes;
        var newHeadOutcomes = newHeadUncertainty!.Outcomes;
        // calculate new probabilities for the new tail since it is simpler because one condition has been removed
        // sum the probabilities of the new tail outcomes along the new head outcomes to get the new probabilities for the new tail outcomes

        var newHeadOutcomeIds = new HashSet<Guid>(newHeadOutcomes.Select(o => o.Id));
        var accumulator = new Dictionary<(Guid outcomeId, string parentKey), double>();
        var originalProbabilities = newTailUncertainty.DiscreteProbabilities;

        foreach (var x in newHeadOutcomes)
        {
            // iterate over the newHeadOutcomes to sum along them
            // example: ParentOutcomeIds = (a1, b1, c1), (a1, b1, c2), (a1, b2, c1), (a1, b2, c2), (a2, b1, c1), (a2, b1, c2), (a2, b2, c1), (a2, b2, c2)
            // if x = b1, then sum the probabilities of (a1, b1, c1), (a1, b1, c2), (a2, b1, c1), (a2, b1, c2) to get the new probability for a1 and a2

            // P(x): marginal probability of this newHead outcome
            var pX = newHeadUncertainty!.DiscreteProbabilities
                .FirstOrDefault(dp => dp.OutcomeId == x.Id)
                ?.Probability ?? 0.0;

            // Accumulate: P(newTail | remainingParents) += P(newTail | remainingParents, x) * P(x)
            foreach (var dp in originalProbabilities.Where(dp => dp.ParentOutcomeIds.Contains(x.Id)))
            {
                var remainingParents = dp.ParentOutcomeIds
                    .Where(id => !newHeadOutcomeIds.Contains(id))
                    .OrderBy(id => id)
                    .ToList();

                var parentKey = string.Join(",", remainingParents);
                var key = (dp.OutcomeId, parentKey);

                accumulator.TryGetValue(key, out var existing);
                accumulator[key] = existing + (dp.Probability ?? 0.0) * pX;
            }
        }

        var projectId = originalProbabilities.FirstOrDefault()?.ProjectId ?? Guid.Empty;
        var newTailProbabilities = accumulator.Select(kvp => new DiscreteProbabilityDto
        {
            Id = Guid.NewGuid(),
            OutcomeId = kvp.Key.outcomeId,
            UncertaintyId = newTailId,
            ProjectId = projectId,
            Probability = kvp.Value,
            ParentOutcomeIds = string.IsNullOrEmpty(kvp.Key.parentKey)
                ? []
                : kvp.Key.parentKey.Split(',').Select(Guid.Parse).ToList()
        }).ToList();

        await Task.Delay(100, ct);
        return newTailProbabilities;
    }
}