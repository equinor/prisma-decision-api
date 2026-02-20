using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Repositories;

public static class EntitiesExtensions
{
    public static ICollection<ProjectRole> Update(this ICollection<ProjectRole> entities, ICollection<ProjectRole> incommingEntities)
    {
        // Update
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;

            entity.ProjectId = inncommingEntity.ProjectId;
            entity.UserId = inncommingEntity.UserId;
            entity.Role = inncommingEntity.Role;
            entity.UpdatedById = inncommingEntity.UpdatedById;
        }

        // delete
        entities = RepositoryUtilities.RemoveMissingFromCollection<ProjectRole, Guid>(incommingEntities, entities);

        // create
        entities = RepositoryUtilities.AddMissingFromCollection<ProjectRole, Guid>(incommingEntities, entities);

        return entities;
    }

    public static ICollection<Objective> Update(this ICollection<Objective> entities, ICollection<Objective> incommingEntities)
    {
        // delete
        entities = RepositoryUtilities.RemoveMissingFromCollection<Objective, Guid>(incommingEntities, entities);

        // create
        entities = RepositoryUtilities.AddMissingFromCollection<Objective, Guid>(incommingEntities, entities);

        // update
        foreach (var entity in entities)
        {
            var incommingEntity = incommingEntities.Where(x => x.Id == entity.Id).First();

            entity.Name = incommingEntity.Name;
            entity.Type = incommingEntity.Type;
            entity.ProjectId = incommingEntity.ProjectId;
            entity.Description = incommingEntity.Description;
            entity.UpdatedById = incommingEntity.UpdatedById;
        }
        return entities;
    }

    public static ICollection<Strategy> Update(this ICollection<Strategy> entities, ICollection<Strategy> incommingEntities)
    {
        // delete
        entities = RepositoryUtilities.RemoveMissingFromCollection<Strategy, Guid>(incommingEntities, entities);

        // create
        entities = RepositoryUtilities.AddMissingFromCollection<Strategy, Guid>(incommingEntities, entities);

        // update
        foreach (var entity in entities)
        {
            var incommingEntity = incommingEntities.Where(x => x.Id == entity.Id).First();


            entity.ProjectId = incommingEntity.ProjectId;
            entity.Description = incommingEntity.Description;
            entity.UpdatedById = incommingEntity.UpdatedById;
            entity.StrategyOptions = entity.StrategyOptions.Update(incommingEntity.StrategyOptions);
        }
        return entities;
    }

    public static ICollection<StrategyOption> Update(this ICollection<StrategyOption> entities, ICollection<StrategyOption> incommingEntities)
    {
        // delete
        var entitiesToRemove = entities.Where(e => !incommingEntities.Any(ie => ie.OptionId == e.OptionId && ie.StrategyId == e.StrategyId)).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            entities.Remove(entityToRemove);
        }

        // create
        var entitiesToAdd = incommingEntities.Where(ie => !entities.Any(e => e.OptionId == ie.OptionId && e.StrategyId == ie.StrategyId)).ToList();
        foreach (var entityToAdd in entitiesToAdd)
        {
            entities.Add(entityToAdd);
        }

        return entities;
    }

    public static Node Update(this Node entity, Node incommingEntity)
    {
        entity.Name = incommingEntity.Name;
        entity.HeadEdges = entity.HeadEdges.Update(incommingEntity.HeadEdges);
        entity.TailEdges = entity.TailEdges.Update(incommingEntity.TailEdges);
        if (incommingEntity.NodeStyle != null && entity.NodeStyle != null)
            entity.NodeStyle = entity.NodeStyle.Update(incommingEntity.NodeStyle);
        return entity;
    }

    public static ICollection<Edge> Update(this ICollection<Edge> entities, ICollection<Edge> incommingEntities)
    {
        entities = RepositoryUtilities.RemoveMissingFromCollection<Edge, Guid>(incommingEntities, entities);
        entities = RepositoryUtilities.AddMissingFromCollection<Edge, Guid>(incommingEntities, entities);
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.HeadId = inncommingEntity.HeadId;
            entity.TailId = inncommingEntity.TailId;
        }
        return entities;
    }

    public static NodeStyle Update(this NodeStyle entity, NodeStyle incommingEntity)
    {
        entity.XPosition = incommingEntity.XPosition;
        entity.YPosition = incommingEntity.YPosition;
        return entity;
    }

    public static Decision Update(this Decision entity, Decision incommingEntity)
    {
        entity.Options = entity.Options.Update(incommingEntity.Options);
        return entity;
    }

    public static ICollection<Option> Update(this ICollection<Option> entities, ICollection<Option> incommingEntities)
    {
        entities = RepositoryUtilities.RemoveMissingFromCollection<Option, Guid>(incommingEntities, entities);
        entities = RepositoryUtilities.AddMissingFromCollection<Option, Guid>(incommingEntities, entities);

        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.Name = inncommingEntity.Name;
            entity.Utility = inncommingEntity.Utility;
        }
        return entities;
    }

    public static Uncertainty Update(this Uncertainty entity, Uncertainty incommingEntity)
    {
        entity.Outcomes = entity.Outcomes.Update(incommingEntity.Outcomes);
        entity.DiscreteProbabilities = entity.DiscreteProbabilities.Update(incommingEntity.DiscreteProbabilities);
        return entity;
    }

    public static ICollection<Outcome> Update(this ICollection<Outcome> entities, ICollection<Outcome> incommingEntities)
    {
        entities = RepositoryUtilities.RemoveMissingFromCollection<Outcome, Guid>(incommingEntities, entities);
        entities = RepositoryUtilities.AddMissingFromCollection<Outcome, Guid>(incommingEntities, entities);

        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.Name = inncommingEntity.Name;
            entity.Utility = inncommingEntity.Utility;
        }
        return entities;
    }

    public static ICollection<DiscreteProbability> Update(this ICollection<DiscreteProbability> entities, ICollection<DiscreteProbability> incommingEntities)
    {
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.Probability = inncommingEntity.Probability;
        }
        return entities;
    }

    public static Utility Update(this Utility entity, Utility incommingEntity)
    {
        entity.DiscreteUtilities = entity.DiscreteUtilities.Update(incommingEntity.DiscreteUtilities);
        return entity;
    }

    public static ICollection<DiscreteUtility> Update(this ICollection<DiscreteUtility> entities, ICollection<DiscreteUtility> incommingEntities)
    {
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.UtilityValue = inncommingEntity.UtilityValue;
        }
        return entities;
    }
}
