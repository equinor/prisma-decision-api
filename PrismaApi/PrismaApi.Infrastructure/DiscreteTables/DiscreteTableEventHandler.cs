using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Infrastructure.DiscreteTables;

public sealed class DiscreteTableEventHandler
{
    public void ProcessSessionChanges(AppDbContext dbContext)
    {
        var sessionInfo = dbContext.DiscreteTableSessionInfo;
        sessionInfo.Clear();

        if (dbContext.IsDiscreteTableEventDisabled)
        {
            return;
        }

        var entries = dbContext.ChangeTracker.Entries().ToList();

        var deletedEntries = entries
            .Where(entry => entry.State == EntityState.Deleted && IsSubscribedDelete(entry.Entity))
            .ToList();

        var modifiedEntries = entries
            .Where(entry => entry.State == EntityState.Modified && IsSubscribedModified(entry.Entity))
            .ToList();

        var addedEntries = entries
            .Where(entry => entry.State == EntityState.Added && IsSubscribedAdded(entry.Entity))
            .ToList();

        if (deletedEntries.Count == 0 && modifiedEntries.Count == 0 && addedEntries.Count == 0)
        {
            return;
        }

        if (deletedEntries.Count > 0)
        {
            ProcessDeletions(dbContext, deletedEntries, sessionInfo);
        }

        if (modifiedEntries.Count > 0)
        {
            ProcessModifications(dbContext, modifiedEntries, sessionInfo);
        }

        if (addedEntries.Count > 0)
        {
            ProcessAdditions(dbContext, addedEntries, sessionInfo);
        }
    }

    private static bool IsSubscribedDelete(object entity) =>
        entity is Edge ||
        entity is DiscreteProbabilityParentOption ||
        entity is DiscreteProbabilityParentOutcome ||
        entity is DiscreteUtilityParentOption ||
        entity is DiscreteUtilityParentOutcome;

    private static bool IsSubscribedModified(object entity) =>
        entity is Issue ||
        entity is Uncertainty ||
        entity is Decision ||
        entity is Edge;

    private static bool IsSubscribedAdded(object entity) =>
        entity is Edge ||
        entity is Option ||
        entity is Outcome;

    private static void ProcessDeletions(
        AppDbContext dbContext,
        List<EntityEntry> deletedEntries,
        DiscreteTableSessionInfo sessionInfo)
    {
        var deletedEdgeIds = new HashSet<Guid>();

        foreach (var entry in deletedEntries)
        {
            switch (entry.Entity)
            {
                case Edge edge:
                    deletedEdgeIds.Add(edge.Id);
                    break;
                case DiscreteProbabilityParentOutcome parentOutcome:
                    sessionInfo.DiscreteProbabilitiesToDelete.Add(parentOutcome.DiscreteProbabilityId);
                    break;
                case DiscreteProbabilityParentOption parentOption:
                    sessionInfo.DiscreteProbabilitiesToDelete.Add(parentOption.DiscreteProbabilityId);
                    break;
                case DiscreteUtilityParentOutcome parentOutcome:
                    sessionInfo.DiscreteUtilitiesToDelete.Add(parentOutcome.DiscreteUtilityId);
                    break;
                case DiscreteUtilityParentOption parentOption:
                    sessionInfo.DiscreteUtilitiesToDelete.Add(parentOption.DiscreteUtilityId);
                    break;
            }
        }

        if (deletedEdgeIds.Count > 0)
        {
            AddAffectedEntitiesFromEdges(dbContext, deletedEdgeIds, sessionInfo);
        }
    }

    private static void ProcessModifications(
        AppDbContext dbContext,
        List<EntityEntry> modifiedEntries,
        DiscreteTableSessionInfo sessionInfo)
    {
        var issuesToSearch = new HashSet<Guid>();
        var issuesPendingStrategyRemoval = new HashSet<Guid>();
        var headIds = new HashSet<Guid>();
        var edgeTailToHeadMapping = new Dictionary<Guid, Guid>();

        foreach (var entry in modifiedEntries)
        {
            switch (entry.Entity)
            {
                case Edge:
                    ProcessEdgeChange(entry, headIds, edgeTailToHeadMapping);
                    break;
                case Issue issue:
                    if (HasBoundaryChange(entry))
                    {
                        issuesToSearch.Add(issue.Id);
                    }

                    if (HasBoundaryChangedFromIn(entry))
                    {
                        issuesPendingStrategyRemoval.Add(issue.Id);
                    }

                    if (HasTypeChangeToOrFromUncertaintyDecision(entry))
                    {
                        sessionInfo.AffectedUncertainties.Add(issue.Id);
                        issuesToSearch.Add(issue.Id);
                    }

                    if (HasTypeChangeFromDecision(entry))
                    {
                        issuesPendingStrategyRemoval.Add(issue.Id);
                    }
                    break;
                case Uncertainty uncertainty:
                    if (HasKeyChange(entry))
                    {
                        issuesToSearch.Add(uncertainty.IssueId);
                    }
                    break;
                case Decision decision:
                    if (HasFocusTypeChange(entry))
                    {
                        issuesToSearch.Add(decision.IssueId);
                    }

                    if (HasFocusTypeChangeFromFocus(entry))
                    {
                        issuesPendingStrategyRemoval.Add(decision.IssueId);
                    }
                    break;
            }
        }

        if (headIds.Count > 0 || edgeTailToHeadMapping.Count > 0)
        {
            AddAffectedEntitiesByNodes(dbContext, headIds, edgeTailToHeadMapping, sessionInfo);
        }

        if (issuesToSearch.Count > 0)
        {
            AddAffectedEntitiesFromIssues(dbContext, issuesToSearch, sessionInfo);
        }

        if (issuesPendingStrategyRemoval.Count > 0)
        {
            foreach (var issueId in issuesPendingStrategyRemoval)
            {
                sessionInfo.IssuesPendingStrategyRemoval.Add(issueId);
            }
        }
    }

    private static void ProcessEdgeChange(
        EntityEntry entry,
        HashSet<Guid> headIds,
        Dictionary<Guid, Guid> edgeTailToHeadMapping)
    {
        var headProperty = entry.Property(nameof(Edge.HeadId));
        var tailProperty = entry.Property(nameof(Edge.TailId));

        if (headProperty.IsModified)
        {
            var originalHeadId = (Guid)headProperty.OriginalValue!;
            var currentHeadId = (Guid)headProperty.CurrentValue!;

            if (originalHeadId != Guid.Empty)
            {
                headIds.Add(originalHeadId);
            }

            if (currentHeadId != Guid.Empty)
            {
                headIds.Add(currentHeadId);
            }
        }

        if (tailProperty.IsModified)
        {
            var originalTailId = (Guid)tailProperty.OriginalValue!;
            var currentTailId = (Guid)tailProperty.CurrentValue!;
            var currentHeadId = (Guid)headProperty.CurrentValue!;

            if (currentHeadId != Guid.Empty)
            {
                if (originalTailId != Guid.Empty)
                {
                    edgeTailToHeadMapping[originalTailId] = currentHeadId;
                }

                if (currentTailId != Guid.Empty)
                {
                    edgeTailToHeadMapping[currentTailId] = currentHeadId;
                }

                headIds.Add(currentHeadId);
            }
        }
    }

    private static void ProcessAdditions(
        AppDbContext dbContext,
        List<EntityEntry> addedEntries,
        DiscreteTableSessionInfo sessionInfo)
    {
        var addedEdgeIds = new HashSet<Guid>();
        var decisionIds = new HashSet<Guid>();
        var uncertaintyIds = new HashSet<Guid>();

        foreach (var entry in addedEntries)
        {
            switch (entry.Entity)
            {
                case Edge edge:
                    addedEdgeIds.Add(edge.Id);
                    break;
                case Option option:
                    decisionIds.Add(option.DecisionId);
                    break;
                case Outcome outcome:
                    uncertaintyIds.Add(outcome.UncertaintyId);
                    break;
            }
        }

        if (addedEdgeIds.Count > 0)
        {
            AddAffectedEntitiesFromEdges(dbContext, addedEdgeIds, sessionInfo);
        }

        if (decisionIds.Count > 0)
        {
            AddAffectedEntitiesFromDecisions(dbContext, decisionIds, sessionInfo);
        }

        if (uncertaintyIds.Count > 0)
        {
            AddAffectedEntitiesFromUncertainties(dbContext, uncertaintyIds, sessionInfo);
        }
    }

    private static void AddAffectedEntitiesFromEdges(
        AppDbContext dbContext,
        HashSet<Guid> edgeIds,
        DiscreteTableSessionInfo sessionInfo)
    {
        var edges = dbContext.Edges
            .Where(edge => edgeIds.Contains(edge.Id))
            .Include(edge => edge.HeadNode!)
                .ThenInclude(node => node.Issue!)
                    .ThenInclude(issue => issue.Uncertainty)
            .Include(edge => edge.HeadNode!)
                .ThenInclude(node => node.Issue!)
                    .ThenInclude(issue => issue.Utility)
            .ToList();

        foreach (var edge in edges)
        {
            var issue = edge.HeadNode?.Issue;
            if (issue == null)
            {
                continue;
            }

            if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty != null)
            {
                sessionInfo.AffectedUncertainties.Add(issue.Uncertainty.Id);
            }

            if (IsIssueType(issue.Type, IssueType.Utility) && issue.Utility != null)
            {
                sessionInfo.AffectedUtilities.Add(issue.Utility.Id);
            }
        }
    }

    private static void AddAffectedEntitiesByNodes(
        AppDbContext dbContext,
        HashSet<Guid> headIds,
        Dictionary<Guid, Guid> edgeTailToHeadMapping,
        DiscreteTableSessionInfo sessionInfo)
    {
        var nodesToGet = new HashSet<Guid>(headIds);
        foreach (var entry in edgeTailToHeadMapping)
        {
            nodesToGet.Add(entry.Key);
            nodesToGet.Add(entry.Value);
        }

        var nodes = dbContext.Nodes
            .Where(node => nodesToGet.Contains(node.Id))
            .Include(node => node.Issue!)
                .ThenInclude(issue => issue.Utility)
            .Include(node => node.Issue!)
                .ThenInclude(issue => issue.Decision)
            .Include(node => node.Issue!)
                .ThenInclude(issue => issue.Uncertainty)
            .ToList();

        var nodeMap = nodes.ToDictionary(node => node.Id, node => node);

        foreach (var entry in edgeTailToHeadMapping)
        {
            if (!nodeMap.TryGetValue(entry.Key, out var tailNode))
            {
                continue;
            }

            if (!IsValidTailNode(tailNode))
            {
                continue;
            }

            if (nodeMap.TryGetValue(entry.Value, out var headNode))
            {
                headIds.Add(headNode.Id);
            }
        }

        foreach (var headId in headIds)
        {
            if (nodeMap.TryGetValue(headId, out var headNode))
            {
                AddAffectedEntity(sessionInfo, headNode);
            }
        }
    }

    private static void AddAffectedEntitiesFromIssues(
        AppDbContext dbContext,
        HashSet<Guid> issueIds,
        DiscreteTableSessionInfo sessionInfo)
    {
        var issues = dbContext.Issues
            .Where(issue => issueIds.Contains(issue.Id))
            .Include(issue => issue.Node!)
                .ThenInclude(node => node.TailEdges)
                    .ThenInclude(edge => edge.HeadNode!)
                        .ThenInclude(node => node.Issue!)
                            .ThenInclude(issue => issue.Uncertainty)
            .Include(issue => issue.Node!)
                .ThenInclude(node => node.TailEdges)
                    .ThenInclude(edge => edge.HeadNode!)
                        .ThenInclude(node => node.Issue!)
                            .ThenInclude(issue => issue.Utility)
            .ToList();

        foreach (var issue in issues)
        {
            if (issue.Node == null)
            {
                continue;
            }

            foreach (var edge in issue.Node.TailEdges)
            {
                var headIssue = edge.HeadNode?.Issue;
                if (headIssue == null)
                {
                    continue;
                }

                if (IsIssueType(headIssue.Type, IssueType.Uncertainty) && headIssue.Uncertainty != null)
                {
                    sessionInfo.AffectedUncertainties.Add(headIssue.Uncertainty.Id);
                }

                if (IsIssueType(headIssue.Type, IssueType.Utility) && headIssue.Utility != null)
                {
                    sessionInfo.AffectedUtilities.Add(headIssue.Utility.Id);
                }
            }
        }
    }

    private static void AddAffectedEntitiesFromDecisions(
        AppDbContext dbContext,
        HashSet<Guid> decisionIds,
        DiscreteTableSessionInfo sessionInfo)
    {
        var decisions = dbContext.Decisions
            .Where(decision => decisionIds.Contains(decision.Id))
            .Include(decision => decision.Issue!)
                .ThenInclude(issue => issue.Node!)
                    .ThenInclude(node => node.TailEdges)
                        .ThenInclude(edge => edge.HeadNode!)
                            .ThenInclude(node => node.Issue!)
                                .ThenInclude(issue => issue.Uncertainty)
            .Include(decision => decision.Issue!)
                .ThenInclude(issue => issue.Node!)
                    .ThenInclude(node => node.TailEdges)
                        .ThenInclude(edge => edge.HeadNode!)
                            .ThenInclude(node => node.Issue!)
                                .ThenInclude(issue => issue.Utility)
            .ToList();

        foreach (var decision in decisions)
        {
            var node = decision.Issue?.Node;
            if (node == null)
            {
                continue;
            }

            foreach (var edge in node.TailEdges)
            {
                var headIssue = edge.HeadNode?.Issue;
                if (headIssue == null)
                {
                    continue;
                }

                if (IsIssueType(headIssue.Type, IssueType.Uncertainty) && headIssue.Uncertainty != null)
                {
                    sessionInfo.AffectedUncertainties.Add(headIssue.Uncertainty.Id);
                }

                if (IsIssueType(headIssue.Type, IssueType.Utility) && headIssue.Utility != null)
                {
                    sessionInfo.AffectedUtilities.Add(headIssue.Utility.Id);
                }
            }
        }
    }

    private static void AddAffectedEntitiesFromUncertainties(
        AppDbContext dbContext,
        HashSet<Guid> uncertaintyIds,
        DiscreteTableSessionInfo sessionInfo)
    {
        var uncertainties = dbContext.Uncertainties
            .Where(uncertainty => uncertaintyIds.Contains(uncertainty.Id))
            .Include(uncertainty => uncertainty.Issue!)
                .ThenInclude(issue => issue.Node!)
                    .ThenInclude(node => node.TailEdges)
                        .ThenInclude(edge => edge.HeadNode!)
                            .ThenInclude(node => node.Issue!)
                                .ThenInclude(issue => issue.Uncertainty)
            .Include(uncertainty => uncertainty.Issue!)
                .ThenInclude(issue => issue.Node!)
                    .ThenInclude(node => node.TailEdges)
                        .ThenInclude(edge => edge.HeadNode!)
                            .ThenInclude(node => node.Issue!)
                                .ThenInclude(issue => issue.Utility)
            .ToList();

        foreach (var uncertainty in uncertainties)
        {
            sessionInfo.AffectedUncertainties.Add(uncertainty.Id);

            var node = uncertainty.Issue?.Node;
            if (node == null)
            {
                continue;
            }

            foreach (var edge in node.TailEdges)
            {
                var headIssue = edge.HeadNode?.Issue;
                if (headIssue == null)
                {
                    continue;
                }

                if (IsIssueType(headIssue.Type, IssueType.Uncertainty) && headIssue.Uncertainty != null)
                {
                    sessionInfo.AffectedUncertainties.Add(headIssue.Uncertainty.Id);
                }
                else if (IsIssueType(headIssue.Type, IssueType.Utility) && headIssue.Utility != null)
                {
                    sessionInfo.AffectedUtilities.Add(headIssue.Utility.Id);
                }
            }
        }
    }

    private static bool IsValidTailNode(Node node)
    {
        var issue = node.Issue;
        if (issue == null)
        {
            return false;
        }

        if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty != null)
        {
            return true;
        }

        if (IsIssueType(issue.Type, IssueType.Decision) && issue.Decision != null)
        {
            return true;
        }

        return false;
    }

    private static void AddAffectedEntity(DiscreteTableSessionInfo sessionInfo, Node node)
    {
        var issue = node.Issue;
        if (issue == null)
        {
            return;
        }

        if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty != null)
        {
            sessionInfo.AffectedUncertainties.Add(issue.Uncertainty.Id);
        }
        else if (IsIssueType(issue.Type, IssueType.Utility) && issue.Utility != null)
        {
            sessionInfo.AffectedUtilities.Add(issue.Utility.Id);
        }
    }

    private static bool HasBoundaryChange(EntityEntry entry)
    {
        var boundaryProperty = entry.Property(nameof(Issue.Boundary));
        if (!boundaryProperty.IsModified)
        {
            return false;
        }

        var original = Normalize(boundaryProperty.OriginalValue as string);
        var current = Normalize(boundaryProperty.CurrentValue as string);
        var outValue = Normalize(Boundary.Out.ToString());

        return original == outValue || current == outValue;
    }

    private static bool HasBoundaryChangedFromIn(EntityEntry entry)
    {
        var boundaryProperty = entry.Property(nameof(Issue.Boundary));
        if (!boundaryProperty.IsModified)
        {
            return false;
        }

        var original = Normalize(boundaryProperty.OriginalValue as string);
        var inValue = Normalize(Boundary.In.ToString());

        return original == inValue;
    }

    private static bool HasTypeChangeToOrFromUncertaintyDecision(EntityEntry entry)
    {
        var typeProperty = entry.Property(nameof(Issue.Type));
        if (!typeProperty.IsModified)
        {
            return false;
        }

        var original = Normalize(typeProperty.OriginalValue as string);
        var current = Normalize(typeProperty.CurrentValue as string);

        return IsTypeUncertaintyOrDecision(original) || IsTypeUncertaintyOrDecision(current);
    }

    private static bool HasTypeChangeFromDecision(EntityEntry entry)
    {
        var typeProperty = entry.Property(nameof(Issue.Type));
        if (!typeProperty.IsModified)
        {
            return false;
        }

        var original = Normalize(typeProperty.OriginalValue as string);
        return original == Normalize(IssueType.Decision.ToString());
    }

    private static bool HasKeyChange(EntityEntry entry)
    {
        var keyProperty = entry.Property(nameof(Uncertainty.IsKey));
        return keyProperty.IsModified;
    }

    private static bool HasFocusTypeChange(EntityEntry entry)
    {
        var typeProperty = entry.Property(nameof(Decision.Type));
        if (!typeProperty.IsModified)
        {
            return false;
        }

        var original = Normalize(typeProperty.OriginalValue as string);
        var current = Normalize(typeProperty.CurrentValue as string);
        var focus = Normalize(DecisionHierarchy.Focus.ToString());

        return original == focus || current == focus;
    }

    private static bool HasFocusTypeChangeFromFocus(EntityEntry entry)
    {
        var typeProperty = entry.Property(nameof(Decision.Type));
        if (!typeProperty.IsModified)
        {
            return false;
        }

        var original = Normalize(typeProperty.OriginalValue as string);
        var focus = Normalize(DecisionHierarchy.Focus.ToString());

        return original == focus;
    }

    private static bool IsIssueType(string? value, IssueType type)
    {
        return Normalize(value) == Normalize(type.ToString());
    }

    private static bool IsTypeUncertaintyOrDecision(string value)
    {
        var normalized = Normalize(value);
        return normalized == Normalize(IssueType.Uncertainty.ToString()) ||
               normalized == Normalize(IssueType.Decision.ToString());
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
}
