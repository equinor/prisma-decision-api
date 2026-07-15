using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure.DiscreteTables;

public sealed class DiscreteTableSessionInfo
{
    public HashSet<Guid> AffectedIssueIds { get; } = new();
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
            AffectedIssueIds.Add(issueId);
    }

    public void EnqueueRestrictionTables(ICollection<Guid> restrictionTableIds)
    {
        foreach (var restrictionTableId in restrictionTableIds)
            AffectedRestrictionTables.Add(restrictionTableId);
    }
}
