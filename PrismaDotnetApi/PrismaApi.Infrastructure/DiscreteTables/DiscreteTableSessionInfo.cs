using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure.DiscreteTables;

// enum to represent the reason why an issue is affected
public enum AffectedIssueReason
{
    Unknown,
    ParentStateAdded,
    OwnOutcomeAdded,
}

public class AffectedIssueObject
{
    public Guid AffectedIssueId { get; set; }
    // provide information on why this issue is affected
    public AffectedIssueReason Reason { get; set; } = AffectedIssueReason.Unknown;

}

public sealed class DiscreteTableSessionInfo
{
    public HashSet<AffectedIssueObject> AffectedIssueIds { get; } = new();
    public HashSet<Guid> AffectedRestrictionTables { get; } = new();

    public bool HasChanges =>
        AffectedIssueIds.Count > 0 || AffectedRestrictionTables.Count > 0;

    public void Clear()
    {
        AffectedIssueIds.Clear();
        AffectedRestrictionTables.Clear();
    }

    public void EnqueueIssues(ICollection<Guid> issueIds)
    {
        foreach (var issueId in issueIds)
            AffectedIssueIds.Add(new AffectedIssueObject { AffectedIssueId = issueId, Reason = AffectedIssueReason.Unknown });
    }

    public void EnqueueRestrictionTables(ICollection<Guid> restrictionTableIds)
    {
        foreach (var restrictionTableId in restrictionTableIds)
            AffectedRestrictionTables.Add(restrictionTableId);
    }
}
