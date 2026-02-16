using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure.DiscreteTables;

public sealed class DiscreteTableSessionInfo
{
    public HashSet<Guid> AffectedUncertainties { get; } = new();
    public HashSet<Guid> AffectedUtilities { get; } = new();
    public HashSet<Guid> IssuesPendingStrategyRemoval { get; } = new();
    public HashSet<Guid> DiscreteProbabilitiesToDelete { get; } = new();
    public HashSet<Guid> DiscreteUtilitiesToDelete { get; } = new();

    public bool HasChanges =>
        AffectedUncertainties.Count > 0 ||
        AffectedUtilities.Count > 0 ||
        IssuesPendingStrategyRemoval.Count > 0 ||
        DiscreteProbabilitiesToDelete.Count > 0 ||
        DiscreteUtilitiesToDelete.Count > 0;

    public void Clear()
    {
        AffectedUncertainties.Clear();
        AffectedUtilities.Clear();
        IssuesPendingStrategyRemoval.Clear();
        DiscreteProbabilitiesToDelete.Clear();
        DiscreteUtilitiesToDelete.Clear();
    }
}
