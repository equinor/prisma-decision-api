using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Utilities;

namespace PrismaDotnetApi.PrismaApi.Domain.Extensions;

public static class DiscreteUtilitiesExtensions
{
	public static void AddUtilitiesColumn(this ICollection<DiscreteUtilityDto> utilities, ValueMetricOutgoingDto addedValueMetric)
	{
		utilities.ValidateUtilities();
		var rows = utilities.SeparateByRow();

		foreach (var row in rows)
		{
			var template = row.First();
			utilities.Add(new DiscreteUtilityDto
			{
				Id = Utilities.GetDeterministicId(template.ProjectId, addedValueMetric.Id, template.ParentOutcomeIds, template.ParentOptionIds),
				ProjectId = template.ProjectId,
				UtilityId = template.UtilityId,
				ParentOptionIds = new List<Guid>(template.ParentOptionIds),
				ParentOutcomeIds = new List<Guid>(template.ParentOutcomeIds),
				ValueMetricId = addedValueMetric.Id,
				UtilityValue = 0
			});
		}
	}

	public static void AddUtilitiesRow(this ICollection<DiscreteUtilityDto> utilities, Guid addedStateId, List<Guid> siblingsOfAddedState)
	{
		utilities.ValidateUtilities();
		var rows = utilities.SeparateByRow();
		var exampleSiblingId = siblingsOfAddedState.First();
		var rowsToDuplicate = rows
			.Where(row => row.Any(utility => utility.ParentOptionIds.Contains(exampleSiblingId)
				|| utility.ParentOutcomeIds.Contains(exampleSiblingId)))
			.ToList();

		foreach (var row in rowsToDuplicate)
		{
			foreach (var utility in row)
			{
				var newUtility = new DiscreteUtilityDto
				{
					Id = Utilities.GetDeterministicId(utility.ProjectId, addedStateId, utility.ParentOutcomeIds, utility.ParentOptionIds),
					ProjectId = utility.ProjectId,
					UtilityId = utility.UtilityId,
					ParentOptionIds = new List<Guid>(utility.ParentOptionIds),
					ParentOutcomeIds = new List<Guid>(utility.ParentOutcomeIds),
					ValueMetricId = utility.ValueMetricId,
					UtilityValue = 0
				};

				if (newUtility.ParentOptionIds.Remove(exampleSiblingId))
				{
					newUtility.ParentOptionIds.Add(addedStateId);
				}
				else if (newUtility.ParentOutcomeIds.Remove(exampleSiblingId))
				{
					newUtility.ParentOutcomeIds.Add(addedStateId);
				}

				utilities.Add(newUtility);
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
}
