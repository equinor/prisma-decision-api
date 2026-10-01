using PrismaApi.Domain.Extensions;

namespace PrismaApi.Domain.Utilities;
public static class IdGenerationUtils
{
    public static Guid GetDeterministicIdProbability(Guid issueId, Guid stateId, List<Guid> parentOutcomeIds, List<Guid> parentOptionIds)
    {
        var combined = $"{issueId}|" +
                    $"StateId:{stateId}|" +
                    $"Outcomes:{string.Join(",", parentOutcomeIds.OrderBy(id => id))}|" +
                    $"Options:{string.Join(",", parentOptionIds.OrderBy(id => id))}";
        return combined.GenerateDeterministicGuid();
    }

    public static Guid GetDeterministicIdUtility(Guid issueId, Guid valueMetricId, List<Guid> parentOutcomeIds, List<Guid> parentOptionIds)
    {
        var combined = $"{issueId}|" +
                    $"ValueMetricId:{valueMetricId}|" +
                    $"Outcomes:{string.Join(",", parentOutcomeIds.OrderBy(id => id))}|" +
                    $"Options:{string.Join(",", parentOptionIds.OrderBy(id => id))}";
        return combined.GenerateDeterministicGuid();
    }
}