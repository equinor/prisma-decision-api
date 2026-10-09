using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Utilities;

namespace PrismaDotnetApi.PrismaApi.Domain.Extensions;

public static class DiscreteUtilitiesExtensions
{
	public static void AddUtilitiesColumn(this ICollection<DiscreteUtilityDto> utilities, Guid issueId)
	{
		utilities.ValidateUtilities();
		var rows = utilities.SeparateByRow();

		foreach (var row in rows)
		{
			var template = row.First();
			utilities.Add(new DiscreteUtilityDto
			{
                Id = IdGenerationUtils.GetDeterministicIdUtility(issueId, DomainConstants.DefaultValueMetricId, template.ParentOutcomeIds, template.ParentOptionIds),
				ProjectId = template.ProjectId,
				UtilityId = template.UtilityId,
				ParentOptionIds = new List<Guid>(template.ParentOptionIds),
				ParentOutcomeIds = new List<Guid>(template.ParentOutcomeIds),
				ValueMetricId = DomainConstants.DefaultValueMetricId,
				UtilityValue = 0
			});
		}
	}

	public static void AddUtilitiesRow(this ICollection<DiscreteUtilityDto> utilities, Guid issueId, Guid addedStateId, List<Guid> siblingsOfAddedState)
	{
		utilities.ValidateUtilities();
		var rows = utilities.SeparateByRow();
		var siblingIdOfAddedState = siblingsOfAddedState.First();
		var rowsToDuplicate = rows
			.Where(row => row.Any(utility => utility.ParentOptionIds.Contains(siblingIdOfAddedState)
				|| utility.ParentOutcomeIds.Contains(siblingIdOfAddedState)))
			.ToList();

		foreach (var row in rowsToDuplicate)
		{
			foreach (var utility in row)
			{
				var newUtility = new DiscreteUtilityDto
				{
					ProjectId = utility.ProjectId,
					UtilityId = utility.UtilityId,
					ParentOptionIds = new List<Guid>(utility.ParentOptionIds),
					ParentOutcomeIds = new List<Guid>(utility.ParentOutcomeIds),
					ValueMetricId = utility.ValueMetricId,
					UtilityValue = 0,
				};

				if (newUtility.ParentOptionIds.Remove(siblingIdOfAddedState))
				{
					newUtility.ParentOptionIds.Add(addedStateId);
				}
				else if (newUtility.ParentOutcomeIds.Remove(siblingIdOfAddedState))
				{
					newUtility.ParentOutcomeIds.Add(addedStateId);
				}

				utilities.Add(newUtility);
				newUtility.Id = IdGenerationUtils.GetDeterministicIdUtility(issueId, DomainConstants.DefaultValueMetricId, newUtility.ParentOutcomeIds, newUtility.ParentOptionIds);
			}
		}
	}

	private static List<IGrouping<string, DiscreteUtilityDto>> SeparateByRow(this ICollection<DiscreteUtilityDto> utilities)
	{
		return utilities.GroupBy(utility =>
			string.Join(",", utility.ParentOptionIds.OrderBy(id => id)
				.Concat(utility.ParentOutcomeIds.OrderBy(id => id))))
			.ToList();
	}

	private static void ValidateUtilities(this ICollection<DiscreteUtilityDto> utilities)
	{
		if (utilities.Count == 0)
		{
			throw new InvalidOperationException("No utilities provided.");
		}

		var firstUtilityId = utilities.First().UtilityId;
		if (utilities.Any(utility => utility.UtilityId != firstUtilityId))
		{
			throw new InvalidOperationException("All utilities must belong to the same utility.");
		}
	}

	public static void AddUtilityParent(
		this ICollection<DiscreteUtilityDto> utilities,
		ICollection<Guid> addedParentStateIds,
		bool parentIsOption
	)
	{
		// Expand each existing row into one copy per state of the new parent, preserving its utility values.
        // The expanded rows replace the original row because every valid row must reference a state of the new parent.

		var utilityRows = utilities.SeparateByRow();
		var newUtilities = new List<DiscreteUtilityDto>();

		foreach (var row in utilityRows)
		{
			foreach (var addedParentStateId in addedParentStateIds)
			{
				var newRow = row.Select(u => new DiscreteUtilityDto
				{
					ProjectId = u.ProjectId,
					UtilityId = u.UtilityId,
					ParentOptionIds = new List<Guid>(u.ParentOptionIds),
					ParentOutcomeIds = new List<Guid>(u.ParentOutcomeIds),
					ValueMetricId = u.ValueMetricId,
					UtilityValue = 0
				}).ToList();

				if (parentIsOption)
				{
					foreach (var utility in newRow)
					{
						utility.ParentOptionIds.Add(addedParentStateId);
					}
				}
				else
				{
					foreach (var utility in newRow)
					{
						utility.ParentOutcomeIds.Add(addedParentStateId);
					}
				}

				newUtilities.AddRange(newRow);
			}
		}

		utilities.Clear();
		foreach (var newUtility in newUtilities)
		{
			utilities.Add(newUtility);
		}
	}
}