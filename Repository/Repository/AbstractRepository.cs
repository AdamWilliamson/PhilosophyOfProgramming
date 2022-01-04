using System.Collections.Generic;

namespace Repository
{
    public abstract class AbstractRepository<TEntity, TKey> : AbstractReadOnlyRepository<TEntity, TKey>, IRepository<TEntity, TKey>
        where TEntity : class
    {
        public void Add(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public void Add(IEnumerable<TEntity> entities)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(TKey id)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(IEnumerable<TEntity> entities)
        {
            throw new System.NotImplementedException();
        }
    }
}
