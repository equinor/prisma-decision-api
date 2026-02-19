using PrismaApi.Domain.Interfaces;

namespace PrismaApi.Application.Repositories;

public static class RepositoryUtilities
{
    public static ICollection<TEntity> RemoveMissingFromCollection<TEntity, TId>(ICollection<TEntity> incommingEntities, ICollection<TEntity> entities) where TEntity : class, IBaseEntity<TId>
    where TId : struct
    {
        var entitiesToRemove = entities.Where(e => !incommingEntities.Any(ie => EqualityComparer<TId>.Default.Equals(ie.Id, e.Id))).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            entities.Remove(entityToRemove);
        }
        return entities;
    }

    public static ICollection<TEntity> AddMissingFromCollection<TEntity, TId>(ICollection<TEntity> incommingEntities, ICollection<TEntity> entities) where TEntity : class, IBaseEntity<TId>
    {
        var entitiesToAdd = incommingEntities.Where(ie => !entities.Any(e => EqualityComparer<TId>.Default.Equals(ie.Id, e.Id))).ToList();
        foreach (var entityToAdd in entitiesToAdd)
        {
            entities.Add(entityToAdd);
        }
        return entities;
    }
}