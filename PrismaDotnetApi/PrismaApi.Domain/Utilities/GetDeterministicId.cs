using PrismaApi.Domain.Extensions;

namespace PrismaApi.Domain.Utilities;
public static class Utilities
{
    public static Guid GetDeterministicId(Guid issueId, Guid stateId, List<Guid> parentOutcomeIds, List<Guid> parentOptionIds)
    {
        var combined = $"{issueId}|" +
                    $"StateId:{stateId}|" +
                    $"Outcomes:{string.Join(",", parentOutcomeIds.OrderBy(id => id))}|" +
                    $"Options:{string.Join(",", parentOptionIds.OrderBy(id => id))}";
        return combined.GenerateDeterministicGuid();
    }
}