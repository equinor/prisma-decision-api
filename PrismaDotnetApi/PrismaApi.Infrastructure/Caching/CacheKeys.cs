namespace PrismaApi.Infrastructure.Caching;

public static class CacheKeys
{
    public static string GetInfluenceDiagramKey(Guid projectId) => $"InfluenceDiagram_{projectId}";
    public static string GetIssuesInProjectKey(Guid projectId) => $"Issues_Project_{projectId}";
    public static string GetEdgesInProjectKey(Guid projectId) => $"Edges_Project_{projectId}";
    public static string GetNodesInProjectKey(Guid projectId) => $"Nodes_Project_{projectId}";
    public static string GetAssessmentKey(Guid projectId) => $"Assessment_Project_{projectId}";
    public static string GetUserKey(string key) => $"user_{key.ToLower()}"; // id internal, name in public
}