
using Microsoft.EntityFrameworkCore;

namespace EventPipelineHandler.Data
{
    public static class DbsetExtensions
    {
        public static async Task InsertOrUpdate<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : IEntityWithIdAndMetaData
        {
            var entityFromDb = await dbSet.FindAsync(entity.Id);

            if (entityFromDb is null)
            {
                dbSet.Add(entity);
            }
            else
            {
                dbSet.Entry(entityFromDb).CurrentValues.SetValues(entity);
            }
        }
        
        public static async Task InsertOrUpdateRange<TEntity>(this DbSet<TEntity> entities, IEnumerable<TEntity> entitiesToPatch) where TEntity : IEntityWithIdAndMetaData
        {
            var entitiesFromDb = await entities.Where(e => entitiesToPatch.Select(e => e.Id).Contains(e.Id)).ToDictionaryAsync(e => e.Id);

            foreach (var entity in entitiesToPatch)
            {
                if (entitiesFromDb.TryGetValue(entity.Id, out var entityFromDb))
                {
                    entities.Entry(entityFromDb).CurrentValues.SetValues(entity);
                }
                else
                {
                    entities.Add(entity);
                }
            }
        }

        public static async Task<TEntity> GetOrCreate<TEntity>(this DbSet<TEntity> dbSet, TEntity entity) where TEntity : IEntityWithIdAndMetaData
        {
            var entityFromDb = await dbSet.FirstOrDefaultAsync(e => e.Id == entity.Id);

            if (entityFromDb is null)
            {
                dbSet.Add(entity);
            }

            return entityFromDb ?? entity;
        }
    }
}
