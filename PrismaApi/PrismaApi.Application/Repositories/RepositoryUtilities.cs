using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Application.Repositories;

public static class RepositoryUtilities
{
    public static ICollection<TEntity> RemoveMissingFromCollection<TEntity, TId>(ICollection<TEntity> incomingEntities, ICollection<TEntity> entities) where TEntity : class, IBaseEntity<TId>
    where TId : struct
    {
        var entitiesToRemove = entities.Where(e => !incomingEntities.Any(ie => EqualityComparer<TId>.Default.Equals(ie.Id, e.Id))).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            entities.Remove(entityToRemove);
        }
        return entities;
    }

    public static ICollection<TEntity> AddMissingFromCollection<TEntity, TId>(ICollection<TEntity> incomingEntities, ICollection<TEntity> entities) where TEntity : class, IBaseEntity<TId>
    {
        var entitiesToAdd = incomingEntities.Where(ie => !entities.Any(e => EqualityComparer<TId>.Default.Equals(ie.Id, e.Id))).ToList();
        foreach (var entityToAdd in entitiesToAdd)
        {
            entities.Add(entityToAdd);
        }
        return entities;
    }

    public static void RemoveMissingFromCollectionMutate<TEntity, TId>(ICollection<TEntity> incomingEntities, ICollection<TEntity> entities) where TEntity : class, IBaseEntity<TId>
    where TId : struct
    {
        var entitiesToRemove = entities.Where(e => !incomingEntities.Any(ie => EqualityComparer<TId>.Default.Equals(ie.Id, e.Id))).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            entities.Remove(entityToRemove);
        }
    }

    public static void AddMissingFromCollectionMutate<TEntity, TId>(ICollection<TEntity> incomingEntities, ICollection<TEntity> entities, DbContext context) where TEntity : class, IBaseEntity<TId>
    {
        var entitiesToAdd = incomingEntities.Where(ie => !entities.Any(e => EqualityComparer<TId>.Default.Equals(ie.Id, e.Id))).ToList();
        foreach (var entityToAdd in entitiesToAdd)
        {
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
    }

    public static ICollection<TEntity> GetEntitiesToBeAdded<TEntity, TId>(ICollection<TEntity> incomingEntities, ICollection<TEntity> entities) where TEntity : class, IBaseEntity<TId>
    {
        return incomingEntities
            .Where(ie => !entities.Any(e => EqualityComparer<TId>.Default.Equals(ie.Id, e.Id)))
            .ToList();
    }

    public static ICollection<TEntity> GetEntitiesToBeDeleted<TEntity, TId>(ICollection<TEntity> incomingEntities, ICollection<TEntity> entities) where TEntity : class, IBaseEntity<TId>
    {
        return entities
            .Where(e => !incomingEntities.Any(ie => EqualityComparer<TId>.Default.Equals(ie.Id, e.Id)))
            .ToList();
    }
    public static bool IsDecisionMovedOutOfStrategyTable(Decision entity, Decision incomingEntity)
    {
        if (entity.Type != incomingEntity.Type && entity.Type == DecisionHierarchy.Focus.ToString()) return true;
        return false;
    }
    public static bool IsDecisionMovedOutOfStrategyTable(Issue entity, Issue incomingEntity)
    {
        if (entity.Type != incomingEntity.Type && entity.Type == IssueType.Decision.ToString()) return true;
        if (entity.Boundary != incomingEntity.Boundary && incomingEntity.Boundary == Boundary.Out.ToString()) return true;
        if (entity.Decision != null && incomingEntity.Decision != null && entity.Decision.Type != incomingEntity.Decision.Type && entity.Decision.Type == DecisionHierarchy.Focus.ToString()) return true;
        return false;
    }
}