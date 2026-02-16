using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Infrastructure.DiscreteTables;

public sealed class DiscreteTableRecalculator
{
    public async Task RecalculateAsync(
        AppDbContext dbContext,
        DiscreteTableSessionInfo sessionInfo,
        CancellationToken cancellationToken)
    {
        if (!sessionInfo.HasChanges)
        {
            return;
        }

        if (dbContext.IsDiscreteTableEventDisabled)
        {
            return;
        }

        dbContext.IsDiscreteTableEventDisabled = true;
        try
        {
            if (sessionInfo.DiscreteProbabilitiesToDelete.Count > 0)
            {
                await DeleteDiscreteProbabilitiesAsync(dbContext, sessionInfo.DiscreteProbabilitiesToDelete, cancellationToken);
            }

            if (sessionInfo.DiscreteUtilitiesToDelete.Count > 0)
            {
                await DeleteDiscreteUtilitiesAsync(dbContext, sessionInfo.DiscreteUtilitiesToDelete, cancellationToken);
            }

            foreach (var uncertaintyId in sessionInfo.AffectedUncertainties)
            {
                await RecalculateUncertaintyAsync(dbContext, uncertaintyId, cancellationToken);
            }

            foreach (var utilityId in sessionInfo.AffectedUtilities)
            {
                await RecalculateUtilityAsync(dbContext, utilityId, cancellationToken);
            }

            if (sessionInfo.IssuesPendingStrategyRemoval.Count > 0)
            {
                await RemoveStrategyOptionsOutOfScopeAsync(dbContext, sessionInfo.IssuesPendingStrategyRemoval, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            dbContext.IsDiscreteTableEventDisabled = false;
            sessionInfo.Clear();
        }
    }

    private static async Task DeleteDiscreteProbabilitiesAsync(
        AppDbContext dbContext,
        HashSet<Guid> probabilityIds,
        CancellationToken cancellationToken)
    {
        await dbContext.DiscreteProbabilityParentOutcomes
            .Where(parent => probabilityIds.Contains(parent.DiscreteProbabilityId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.DiscreteProbabilityParentOptions
            .Where(parent => probabilityIds.Contains(parent.DiscreteProbabilityId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.DiscreteProbabilities
            .Where(probability => probabilityIds.Contains(probability.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static async Task DeleteDiscreteUtilitiesAsync(
        AppDbContext dbContext,
        HashSet<Guid> utilityIds,
        CancellationToken cancellationToken)
    {
        await dbContext.DiscreteUtilityParentOutcomes
            .Where(parent => utilityIds.Contains(parent.DiscreteUtilityId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.DiscreteUtilityParentOptions
            .Where(parent => utilityIds.Contains(parent.DiscreteUtilityId))
            .ExecuteDeleteAsync(cancellationToken);

        await dbContext.DiscreteUtilities
            .Where(utility => utilityIds.Contains(utility.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static async Task RecalculateUncertaintyAsync(
        AppDbContext dbContext,
        Guid uncertaintyId,
        CancellationToken cancellationToken)
    {
        var entity = await LoadUncertaintyAsync(dbContext, uncertaintyId, cancellationToken);
        if (entity == null)
        {
            return;
        }

        entity.DiscreteProbabilities.Clear();

        var parentOutcomesList = new List<List<Guid>>();
        var parentOptionsList = new List<List<Guid>>();

        var edges = entity.Issue?.Node?.HeadEdges ?? Array.Empty<Edge>();
        var distinctEdges = edges
            .GroupBy(edge => new { edge.TailId, edge.HeadId })
            .Select(group => group.First())
            .ToList();

        foreach (var edge in distinctEdges)
        {
            var issue = edge.TailNode?.Issue;
            if (issue == null || !IsBoundaryInScope(issue.Boundary))
            {
                continue;
            }

            if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty?.IsKey == true)
            {
                parentOutcomesList.Add(issue.Uncertainty.Outcomes.Select(outcome => outcome.Id).ToList());
            }
            else if (IsIssueType(issue.Type, IssueType.Decision) && IsDecisionFocus(issue.Decision?.Type))
            {
                parentOptionsList.Add(issue.Decision.Options.Select(option => option.Id).ToList());
            }
        }

        if (parentOutcomesList.Count == 0 && parentOptionsList.Count == 0)
        {
            foreach (var outcome in entity.Outcomes)
            {
                entity.DiscreteProbabilities.Add(new DiscreteProbability
                {
                    Id = Guid.NewGuid(),
                    UncertaintyId = entity.Id,
                    OutcomeId = outcome.Id,
                    Probability = 0
                });
            }

            return;
        }

        var parentCombinations = BuildCombinations(parentOutcomesList.Concat(parentOptionsList).ToList());
        var allOutcomes = new HashSet<Guid>(parentOutcomesList.SelectMany(list => list));
        var allOptions = new HashSet<Guid>(parentOptionsList.SelectMany(list => list));

        foreach (var outcome in entity.Outcomes)
        {
            foreach (var combination in parentCombinations)
            {
                var probabilityId = Guid.NewGuid();
                var parentOutcomeIds = combination.Where(id => allOutcomes.Contains(id)).ToList();
                var parentOptionIds = combination.Where(id => allOptions.Contains(id)).ToList();

                var probability = new DiscreteProbability
                {
                    Id = probabilityId,
                    UncertaintyId = entity.Id,
                    OutcomeId = outcome.Id,
                    Probability = 0,
                    ParentOutcomes = parentOutcomeIds
                        .Select(id => new DiscreteProbabilityParentOutcome
                        {
                            DiscreteProbabilityId = probabilityId,
                            ParentOutcomeId = id
                        })
                        .ToList(),
                    ParentOptions = parentOptionIds
                        .Select(id => new DiscreteProbabilityParentOption
                        {
                            DiscreteProbabilityId = probabilityId,
                            ParentOptionId = id
                        })
                        .ToList()
                };

                entity.DiscreteProbabilities.Add(probability);
            }
        }
    }

    private static async Task RecalculateUtilityAsync(
        AppDbContext dbContext,
        Guid utilityId,
        CancellationToken cancellationToken)
    {
        var entity = await LoadUtilityAsync(dbContext, utilityId, cancellationToken);
        if (entity == null)
        {
            return;
        }

        entity.DiscreteUtilities.Clear();

        var parentOutcomesList = new List<List<Guid>>();
        var parentOptionsList = new List<List<Guid>>();

        var edges = entity.Issue?.Node?.HeadEdges ?? Array.Empty<Edge>();
        var distinctEdges = edges
            .GroupBy(edge => new { edge.TailId, edge.HeadId })
            .Select(group => group.First())
            .ToList();

        foreach (var edge in distinctEdges)
        {
            var issue = edge.TailNode?.Issue;
            if (issue == null || !IsBoundaryInScope(issue.Boundary))
            {
                continue;
            }

            if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty?.IsKey == true)
            {
                parentOutcomesList.Add(issue.Uncertainty.Outcomes.Select(outcome => outcome.Id).ToList());
            }
            else if (IsIssueType(issue.Type, IssueType.Decision) && IsDecisionFocus(issue.Decision?.Type))
            {
                parentOptionsList.Add(issue.Decision.Options.Select(option => option.Id).ToList());
            }
        }

        if (parentOutcomesList.Count == 0 && parentOptionsList.Count == 0)
        {
            return;
        }

        var parentCombinations = BuildCombinations(parentOutcomesList.Concat(parentOptionsList).ToList());
        var allOutcomes = new HashSet<Guid>(parentOutcomesList.SelectMany(list => list));
        var allOptions = new HashSet<Guid>(parentOptionsList.SelectMany(list => list));

        foreach (var combination in parentCombinations)
        {
            var utilityIdValue = Guid.NewGuid();
            var parentOutcomeIds = combination.Where(id => allOutcomes.Contains(id)).ToList();
            var parentOptionIds = combination.Where(id => allOptions.Contains(id)).ToList();

            var utility = new DiscreteUtility
            {
                Id = utilityIdValue,
                UtilityId = entity.Id,
                ValueMetricId = DomainConstants.DefaultValueMetricId,
                UtilityValue = 0,
                ParentOutcomes = parentOutcomeIds
                    .Select(id => new DiscreteUtilityParentOutcome
                    {
                        DiscreteUtilityId = utilityIdValue,
                        ParentOutcomeId = id
                    })
                    .ToList(),
                ParentOptions = parentOptionIds
                    .Select(id => new DiscreteUtilityParentOption
                    {
                        DiscreteUtilityId = utilityIdValue,
                        ParentOptionId = id
                    })
                    .ToList()
            };

            entity.DiscreteUtilities.Add(utility);
        }
    }

    private static async Task RemoveStrategyOptionsOutOfScopeAsync(
        AppDbContext dbContext,
        HashSet<Guid> issueIds,
        CancellationToken cancellationToken)
    {
        var issues = await dbContext.Issues
            .Where(issue => issueIds.Contains(issue.Id))
            .Include(issue => issue.Decision!)
                .ThenInclude(decision => decision.Options)
                    .ThenInclude(option => option.StrategyOptions)
            .ToListAsync(cancellationToken);

        foreach (var issue in issues)
        {
            if (issue.Decision?.Options == null)
            {
                continue;
            }

            foreach (var option in issue.Decision.Options)
            {
                if (option.StrategyOptions.Count == 0)
                {
                    continue;
                }

                dbContext.StrategyOptions.RemoveRange(option.StrategyOptions);
            }
        }
    }

    private static async Task<Uncertainty?> LoadUncertaintyAsync(
        AppDbContext dbContext,
        Guid uncertaintyId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Uncertainties
            .Where(uncertainty => uncertainty.Id == uncertaintyId)
            .Include(uncertainty => uncertainty.Outcomes)
            .Include(uncertainty => uncertainty.DiscreteProbabilities)
                .ThenInclude(probability => probability.ParentOptions)
            .Include(uncertainty => uncertainty.DiscreteProbabilities)
                .ThenInclude(probability => probability.ParentOutcomes)
            .Include(uncertainty => uncertainty.Issue!)
                .ThenInclude(issue => issue.Node!)
                    .ThenInclude(node => node.HeadEdges)
                        .ThenInclude(edge => edge.TailNode!)
                            .ThenInclude(node => node.Issue!)
                                .ThenInclude(issue => issue.Uncertainty!)
                                    .ThenInclude(uncertainty => uncertainty.Outcomes)
            .Include(uncertainty => uncertainty.Issue!)
                .ThenInclude(issue => issue.Node!)
                    .ThenInclude(node => node.HeadEdges)
                        .ThenInclude(edge => edge.TailNode!)
                            .ThenInclude(node => node.Issue!)
                                .ThenInclude(issue => issue.Decision!)
                                    .ThenInclude(decision => decision.Options)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static async Task<Utility?> LoadUtilityAsync(
        AppDbContext dbContext,
        Guid utilityId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Utilities
            .Where(utility => utility.Id == utilityId)
            .Include(utility => utility.DiscreteUtilities)
                .ThenInclude(discreteUtility => discreteUtility.ParentOptions)
            .Include(utility => utility.DiscreteUtilities)
                .ThenInclude(discreteUtility => discreteUtility.ParentOutcomes)
            .Include(utility => utility.Issue!)
                .ThenInclude(issue => issue.Node!)
                    .ThenInclude(node => node.HeadEdges)
                        .ThenInclude(edge => edge.TailNode!)
                            .ThenInclude(node => node.Issue!)
                                .ThenInclude(issue => issue.Uncertainty!)
                                    .ThenInclude(uncertainty => uncertainty.Outcomes)
            .Include(utility => utility.Issue!)
                .ThenInclude(issue => issue.Node!)
                    .ThenInclude(node => node.HeadEdges)
                        .ThenInclude(edge => edge.TailNode!)
                            .ThenInclude(node => node.Issue!)
                                .ThenInclude(issue => issue.Decision!)
                                    .ThenInclude(decision => decision.Options)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static List<List<Guid>> BuildCombinations(List<List<Guid>> groups)
    {
        var results = new List<List<Guid>> { new List<Guid>() };

        foreach (var group in groups)
        {
            var next = new List<List<Guid>>();
            foreach (var existing in results)
            {
                foreach (var item in group)
                {
                    var combination = new List<Guid>(existing) { item };
                    next.Add(combination);
                }
            }

            results = next;
        }

        return results;
    }

    private static bool IsBoundaryInScope(string? boundary)
    {
        var normalized = Normalize(boundary);
        return normalized == Normalize(Boundary.In.ToString()) ||
               normalized == Normalize(Boundary.On.ToString());
    }

    private static bool IsIssueType(string? value, IssueType type)
    {
        return Normalize(value) == Normalize(type.ToString());
    }

    private static bool IsDecisionFocus(string? value)
    {
        return Normalize(value) == Normalize(DecisionHierarchy.Focus.ToString());
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
}
