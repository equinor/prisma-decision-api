using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;

namespace PrismaApi.Application.Repositories;

public static class EntitiesExtensions
{
    public static void Update(this ICollection<ProjectRole> entities, ICollection<ProjectRole> incomingEntities, AppDbContext context)
    {
        // delete
        RepositoryUtilities.RemoveMissingFromCollectionMutate<ProjectRole, Guid>(incomingEntities, entities);

        // create
        RepositoryUtilities.AddMissingFromCollectionMutate<ProjectRole, Guid>(incomingEntities, entities, context);

        // Update
        foreach (var entity in entities)
        {
            var inncommingEntity = incomingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;

            entity.ProjectId = inncommingEntity.ProjectId;
            entity.UserId = inncommingEntity.UserId;
            entity.Role = inncommingEntity.Role;
            entity.UpdatedById = inncommingEntity.UpdatedById;
        }

    }

    public static void Update(this ICollection<Objective> entities, ICollection<Objective> incomingEntities, AppDbContext context)
    {
        // delete
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Objective, Guid>(incomingEntities, entities);

        // create
        RepositoryUtilities.AddMissingFromCollectionMutate<Objective, Guid>(incomingEntities, entities, context);

        // update
        foreach (var entity in entities)
        {
            var incomingEntity = incomingEntities.Where(x => x.Id == entity.Id).First();

            entity.Name = incomingEntity.Name;
            entity.Type = incomingEntity.Type;
            entity.ProjectId = incomingEntity.ProjectId;
            entity.Description = incomingEntity.Description;
            entity.UpdatedById = incomingEntity.UpdatedById;
        }
    }

    public static void Update(this ICollection<Strategy> entities, ICollection<Strategy> incomingEntities, AppDbContext context)
    {
        // delete
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Strategy, Guid>(incomingEntities, entities);

        // create
        RepositoryUtilities.AddMissingFromCollectionMutate<Strategy, Guid>(incomingEntities, entities, context);

        // update
        foreach (var entity in entities)
        {
            var incomingEntity = incomingEntities.Where(x => x.Id == entity.Id).First();


            entity.ProjectId = incomingEntity.ProjectId;
            entity.Name = incomingEntity.Name;
            entity.Description = incomingEntity.Description;
            entity.Rationale = incomingEntity.Rationale;
            entity.Icon = incomingEntity.Icon;
            entity.IconColor = incomingEntity.IconColor;
            entity.UpdatedById = incomingEntity.UpdatedById;
            entity.StrategyOptions.Update(incomingEntity.StrategyOptions, context);
        }
    }

    public static void Update(this ICollection<StrategyOption> entities, ICollection<StrategyOption> incomingEntities, AppDbContext context)
    {
        // delete
        var entitiesToRemove = entities.Where(e => !incomingEntities.Any(ie => ie.OptionId == e.OptionId && ie.StrategyId == e.StrategyId)).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            entities.Remove(entityToRemove);
        }

        // create
        var entitiesToAdd = incomingEntities.Where(ie => !entities.Any(e => e.OptionId == ie.OptionId && e.StrategyId == ie.StrategyId)).ToList();
        foreach (var entityToAdd in entitiesToAdd)
        {
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
    }

    public static Node Update(this Node entity, Node incomingEntity)
    {
        entity.ProjectId = incomingEntity.ProjectId;
        entity.IssueId = incomingEntity.IssueId;
        entity.Name = incomingEntity.Name;
        if (incomingEntity.NodeStyle != null && entity.NodeStyle != null)
            entity.NodeStyle = entity.NodeStyle.Update(incomingEntity.NodeStyle);
        return entity;
    }

    public static NodeStyle Update(this NodeStyle entity, NodeStyle incomingEntity)
    {
        entity.NodeId = incomingEntity.NodeId;
        entity.XPosition = incomingEntity.XPosition;
        entity.YPosition = incomingEntity.YPosition;
        return entity;
    }

    public static async Task<Decision> Update(this Decision entity, Decision incomingEntity, AppDbContext context, IDiscreteTableRuleEventHandler? ruleTrigger = null)
    {
        if (entity.Type != incomingEntity.Type && incomingEntity.Type != DecisionHierarchy.Focus.ToString() && ruleTrigger != null)
            await ruleTrigger.ParentIssuesChangedAsync([entity.IssueId]);
        await entity.RemoveOutOfScopeStrategyOptions(incomingEntity, context);
        entity.IssueId = incomingEntity.IssueId;
        entity.Type = incomingEntity.Type;
        await entity.Options.Update(incomingEntity.Options, context, ruleTrigger);
        return entity;
    }

    public static async Task Update(this ICollection<Option> entities, ICollection<Option> incommingEntities, AppDbContext context, IDiscreteTableRuleEventHandler? ruleTrigger = null, CancellationToken ct = default)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Option, Guid>(incomingEntities, entities);
        var entitiesToAdd = RepositoryUtilities.GetEntitiesToBeAdded<Option, Guid>(incomingEntities, entities);
        foreach (var entityToAdd in entitiesToAdd)
        {
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
        if (ruleTrigger != null)
            await ruleTrigger.OnDecisionOptionsAddedAsync([.. entitiesToAdd.Select(e => e.DecisionId)], ct);

        foreach (var entity in entities)
        {
            var inncommingEntity = incomingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.DecisionId = inncommingEntity.DecisionId;
            entity.Name = inncommingEntity.Name;
            entity.Utility = inncommingEntity.Utility;
        }
    }

    public static async Task<Uncertainty> Update(this Uncertainty entity, Uncertainty incommingEntity, AppDbContext context, IDiscreteTableRuleEventHandler? ruleTrigger = null, CancellationToken ct = default)
    {
        if (entity.IsKey != incommingEntity.IsKey && ruleTrigger != null)
            await ruleTrigger.ParentIssuesChangedAsync([entity.IssueId], ct);

        await entity.Outcomes.Update(incommingEntity.Outcomes, context, ruleTrigger, ct);
        entity.IsKey = incommingEntity.IsKey;
        entity.DiscreteProbabilities.Update(incommingEntity.DiscreteProbabilities, context);
        return entity;
    }

    public static async Task Update(this ICollection<Outcome> entities, ICollection<Outcome> incommingEntities, AppDbContext context, IDiscreteTableRuleEventHandler? ruleTrigger = null, CancellationToken ct = default)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Outcome, Guid>(incomingEntities, entities);
        var entitiesToAdd = RepositoryUtilities.GetEntitiesToBeAdded<Outcome, Guid>(incomingEntities, entities);
        foreach (var entityToAdd in entitiesToAdd)
        {
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
        if (ruleTrigger != null)
            await ruleTrigger.OnUncertaintyOutcomesAddedAsync([.. entitiesToAdd.Select(e => e.UncertaintyId)], ct);

        foreach (var entity in entities)
        {
            var inncommingEntity = incomingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.UncertaintyId = inncommingEntity.UncertaintyId;
            entity.Name = inncommingEntity.Name;
            entity.Utility = inncommingEntity.Utility;
        }
    }

    public static void Update(this ICollection<DiscreteProbability> entities, ICollection<DiscreteProbability> incomingEntities, AppDbContext context)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<DiscreteProbability, Guid>(incomingEntities, entities);
        RepositoryUtilities.AddMissingFromCollectionMutate<DiscreteProbability, Guid>(incomingEntities, entities, context);
        foreach (var entity in entities)
        {
            var inncommingEntity = incomingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.Probability = inncommingEntity.Probability;
        }
    }

    public static Utility Update(this Utility entity, Utility incomingEntity, AppDbContext context)
    {
        entity.IssueId = incomingEntity.IssueId;
        entity.DiscreteUtilities.Update(incomingEntity.DiscreteUtilities, context);
        return entity;
    }

    public static void Update(this ICollection<DiscreteUtility> entities, ICollection<DiscreteUtility> incomingEntities, AppDbContext context)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<DiscreteUtility, Guid>(incomingEntities, entities);
        RepositoryUtilities.AddMissingFromCollectionMutate<DiscreteUtility, Guid>(incomingEntities, entities, context);
        foreach (var entity in entities)
        {
            var inncommingEntity = incomingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.UtilityValue = inncommingEntity.UtilityValue;
        }
    }

  public static async Task RemoveOutOfScopeStrategyOptions(this Issue entity, Issue incomingEntity, AppDbContext context)
    {
        if (!RepositoryUtilities.IsDecisionMovedOutOfStrategyTable(entity, incomingEntity)) return;
        await RemoveStrategyOptions(context, e => e.Option!.Decision!.IssueId == entity.Id);
    }

    public static async Task RemoveOutOfScopeStrategyOptions(this Decision entity, Decision incomingEntity, AppDbContext context)
    {
        if (!RepositoryUtilities.IsDecisionMovedOutOfStrategyTable(entity, incomingEntity)) return;
        await RemoveStrategyOptions(context, e => e.Option!.DecisionId == entity.Id);
    }

    private static async Task RemoveStrategyOptions(AppDbContext context, Expression<Func<StrategyOption, bool>> predicate)
    {
        var strategyOptionsToBeRemoved = await context.StrategyOptions
            .Where(predicate)
            .ToListAsync();
        if (strategyOptionsToBeRemoved.Count != 0)
        {
            context.StrategyOptions.RemoveRange(strategyOptionsToBeRemoved);
            await context.SaveChangesAsync();
        }
    }
}
