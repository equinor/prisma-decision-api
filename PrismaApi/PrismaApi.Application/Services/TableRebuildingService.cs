using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Services;

public class TableRebuildingService
{
    protected readonly AppDbContext DbContext;
    public TableRebuildingService(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task RebuildTablesAsync()
    {
        var issueIds = DbContext.DiscreteTableSessionInfo.AffectedIssueIds;
        if (issueIds.Count == 0 || DbContext.IsDiscreteTableEventDisabled)
        {
            return;
        }
        await RebuildIssuesFromIssueIds(issueIds);
    }

    public async Task RebuildIssuesFromIssueIds(ICollection<Guid> issueIds)
    {
        var issues = await DbContext.Issues
            .Where(issue => issueIds.Contains(issue.Id))
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.Outcomes)
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.DiscreteProbabilities)
                    .ThenInclude(probability => probability.ParentOutcomes)
            .Include(issue => issue.Uncertainty!)
                .ThenInclude(uncertainty => uncertainty.DiscreteProbabilities)
                    .ThenInclude(probability => probability.ParentOptions)
            .Include(issue => issue.Utility!)
                .ThenInclude(utility => utility.DiscreteUtilities)
                    .ThenInclude(utility => utility.ParentOutcomes)
            .Include(issue => issue.Utility!)
                .ThenInclude(utility => utility.DiscreteUtilities)
                    .ThenInclude(utility => utility.ParentOptions)
            .Include(issue => issue.Node!)
                .ThenInclude(node => node.HeadEdges)
                    .ThenInclude(edge => edge.TailNode!)
                        .ThenInclude(node => node.Issue!)
                            .ThenInclude(parentIssue => parentIssue.Uncertainty!)
                                .ThenInclude(uncertainty => uncertainty.Outcomes)
            .Include(issue => issue.Node!)
                .ThenInclude(node => node.HeadEdges)
                    .ThenInclude(edge => edge.TailNode!)
                        .ThenInclude(node => node.Issue!)
                            .ThenInclude(parentIssue => parentIssue.Decision!)
                                .ThenInclude(decision => decision.Options)
            .ToListAsync();

        foreach (var issue in issues)
        {
            if (IsIssueType(issue.Type, IssueType.Uncertainty) && issue.Uncertainty != null)
            {

                await RebuildUncertaintyTable(issue);
            }
            else if (IsIssueType(issue.Type, IssueType.Utility) && issue.Utility != null)
            {
                await RebuildUtilityTable(issue);
            }
        }

        await DbContext.SaveChangesAsync();
    }

    private async Task RebuildUncertaintyTable(Issue issue)
    {
        var uncertainty = issue.Uncertainty;
        if (uncertainty == null)
        {
            return;
        }
        RemoveDiscreteProbabilities(uncertainty.DiscreteProbabilities);
        uncertainty.DiscreteProbabilities.Clear();

        var (parentOutcomesList, parentOptionsList) = CollectParents(issue);

        if (parentOutcomesList.Count == 0 && parentOptionsList.Count == 0)
        {
            foreach (var outcome in uncertainty.Outcomes)
            {
                var newEntry = new DiscreteProbability
                {
                    Id = Guid.NewGuid(),
                    OutcomeId = outcome.Id,
                    UncertaintyId = uncertainty.Id,
                    Probability = 0
                };
                //Entry(newEntry).State = EntityState.Added;
                //uncertainty.DiscreteProbabilities.Add(newEntry);
                await DbContext.DiscreteProbabilities.AddAsync(newEntry);
            }
            return;
        }

        var parentCombinations = BuildCombinations(parentOutcomesList.Concat(parentOptionsList).ToList());
        var allOutcomes = new HashSet<Guid>(parentOutcomesList.SelectMany(list => list));
        var allOptions = new HashSet<Guid>(parentOptionsList.SelectMany(list => list));


        foreach (var outcome in uncertainty.Outcomes)
        {
            foreach (var combination in parentCombinations)
            {
                var parentOutcomeIds = combination.Where(id => allOutcomes.Contains(id)).OrderBy(id => id).ToList();
                var parentOptionIds = combination.Where(id => allOptions.Contains(id)).OrderBy(id => id).ToList();

                var probabilityId = Guid.NewGuid();

                var newEntity = new DiscreteProbability
                {
                    Id = probabilityId,
                    OutcomeId = outcome.Id,
                    UncertaintyId = uncertainty.Id,
                    Probability = 0,
                };
                DbContext.DiscreteProbabilities.Add(newEntity);


                var parentOptions = parentOptionIds.Select(x =>
                    new DiscreteProbabilityParentOption
                    {
                        ParentOptionId = x,
                        DiscreteProbabilityId = probabilityId,
                    }
                ).ToList();

                await DbContext.DiscreteProbabilityParentOptions.AddRangeAsync(parentOptions);

                var parentOutcomes = parentOutcomeIds.Select(x =>
                    new DiscreteProbabilityParentOutcome
                    {
                        ParentOutcomeId = x,
                        DiscreteProbabilityId = probabilityId,
                    }
                ).ToList();

                await DbContext.DiscreteProbabilityParentOutcomes.AddRangeAsync(parentOutcomes);
            }
        }
    }

    private async Task RebuildUtilityTable(Issue issue)
    {
        var utility = issue.Utility;
        if (utility == null)
        {
            return;
        }

        var (parentOutcomesList, parentOptionsList) = CollectParents(issue);

        RemoveDiscreteUtilities(utility.DiscreteUtilities);
        utility.DiscreteUtilities.Clear();
        if (parentOutcomesList.Count == 0 && parentOptionsList.Count == 0)
        {
            return;
        }

        var parentCombinations = BuildCombinations(parentOutcomesList.Concat(parentOptionsList).ToList());
        var allOutcomes = new HashSet<Guid>(parentOutcomesList.SelectMany(list => list));
        var allOptions = new HashSet<Guid>(parentOptionsList.SelectMany(list => list));

        var desiredKeys = new HashSet<string>();
        var desiredMap = new Dictionary<string, (List<Guid> ParentOutcomes, List<Guid> ParentOptions)>();

        foreach (var combination in parentCombinations)
        {
            var parentOutcomeIds = combination.Where(id => allOutcomes.Contains(id)).OrderBy(id => id).ToList();
            var parentOptionIds = combination.Where(id => allOptions.Contains(id)).OrderBy(id => id).ToList();
            var utilityId = Guid.NewGuid();

            await DbContext.DiscreteUtilities
                .AddAsync(new DiscreteUtility
                {
                    Id = utilityId,
                    ValueMetricId = DomainConstants.DefaultValueMetricId,
                    UtilityId = utility.Id,
                    UtilityValue = 0,
                });


            var parentOptions = parentOptionIds.Select(x =>
                new DiscreteUtilityParentOption
                {
                    ParentOptionId = x,
                    DiscreteUtilityId = utilityId,
                }
            ).ToList();

            await DbContext.DiscreteUtilityParentOptions.AddRangeAsync(parentOptions);

            var parentOutcomes = parentOutcomeIds.Select(x =>
                new DiscreteUtilityParentOutcome
                {
                    ParentOutcomeId = x,
                    DiscreteUtilityId = utilityId,
                }
            ).ToList();

            await DbContext.DiscreteUtilityParentOutcomes.AddRangeAsync(parentOutcomes);
        }

    }

    private (List<List<Guid>> ParentOutcomes, List<List<Guid>> ParentOptions) CollectParents(Issue issue)
    {
        var parentOutcomesList = new List<List<Guid>>();
        var parentOptionsList = new List<List<Guid>>();

        var edges = issue.Node?.HeadEdges ?? Array.Empty<Edge>();
        var distinctEdges = edges
            .GroupBy(edge => new { edge.TailId, edge.HeadId })
            .Select(group => group.First())
            .ToList();

        foreach (var edge in distinctEdges)
        {
            var parentIssue = edge.TailNode?.Issue;
            if (parentIssue == null || !IsBoundaryInScope(parentIssue.Boundary))
            {
                continue;
            }

            if (IsIssueType(parentIssue.Type, IssueType.Uncertainty) && parentIssue.Uncertainty?.IsKey == true)
            {
                parentOutcomesList.Add(parentIssue.Uncertainty.Outcomes.Select(outcome => outcome.Id).ToList());
            }
            else if (IsIssueType(parentIssue.Type, IssueType.Decision) && IsDecisionFocus(parentIssue.Decision?.Type))
            {
                parentOptionsList.Add(parentIssue.Decision!.Options.Select(option => option.Id).ToList());
            }
        }

        return (parentOutcomesList, parentOptionsList);
    }

    private void RemoveDiscreteProbabilities(IEnumerable<DiscreteProbability> probabilities)
    {
        var probabilityIds = probabilities.Select(probability => probability.Id).ToList();
        if (probabilityIds.Count == 0)
        {
            return;
        }

        DbContext.DiscreteProbabilityParentOutcomes.RemoveRange(
            DbContext.DiscreteProbabilityParentOutcomes.Where(parent => probabilityIds.Contains(parent.DiscreteProbabilityId)));
        DbContext.DiscreteProbabilityParentOptions.RemoveRange(
            DbContext.DiscreteProbabilityParentOptions.Where(parent => probabilityIds.Contains(parent.DiscreteProbabilityId)));
        DbContext.DiscreteProbabilities.RemoveRange(probabilities);
    }

    private void RemoveDiscreteUtilities(IEnumerable<DiscreteUtility> utilities)
    {
        var utilityIds = utilities.Select(utility => utility.Id).ToList();
        if (utilityIds.Count == 0)
        {
            return;
        }

        DbContext.DiscreteUtilityParentOutcomes.RemoveRange(
            DbContext.DiscreteUtilityParentOutcomes.Where(parent => utilityIds.Contains(parent.DiscreteUtilityId)));
        DbContext.DiscreteUtilityParentOptions.RemoveRange(
            DbContext.DiscreteUtilityParentOptions.Where(parent => utilityIds.Contains(parent.DiscreteUtilityId)));
        DbContext.DiscreteUtilities.RemoveRange(utilities);
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
