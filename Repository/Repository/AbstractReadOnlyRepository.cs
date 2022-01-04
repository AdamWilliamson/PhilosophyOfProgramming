using System.Collections.Generic;

namespace Repository
{
    public abstract class AbstractReadOnlyRepository<TEntity, TKey>
        : IReadOnlyRepository<TEntity, TKey>
        where TEntity : class
    {
        //IEnumerable<TEntity> Filter(ISpecification<TEntity> specification);
        //{
        //    var query = context.Set<T>().Where(specification.IsSatisifiedBy());
        //    return query.ToList();
        //}    

        //public IEnumerable<T> Filter(ISpecification<T> spec)
        //{
        //    // fetch a Queryable that includes all expression-based includes
        //    var queryableResultWithIncludes = spec.Includes
        //        .Aggregate(_dbContext.Set<T>().AsQueryable(),
        //            (current, include) => current.Include(include));

        //    // modify the IQueryable to include any string-based include statements
        //    var secondaryResult = spec.IncludeStrings
        //        .Aggregate(queryableResultWithIncludes,
        //            (current, include) => current.Include(include));

        //    // return the result of the query using the specification's criteria expression
        //    return secondaryResult
        //                    .Where(spec.Criteria)
        //                    .AsEnumerable();
        //}

        public IEnumerable<TEntity> Filter(ISpecification<TEntity> specification)
        {
            throw new System.NotImplementedException();
        }

        public TEntity Get(TKey id)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TEntity> List()
        {
            throw new System.NotImplementedException();
        }

        public TEntity Single(ISpecification<TEntity> specification)
        {
            throw new System.NotImplementedException();
        }

        public TEntity Single(TKey id)
        {
            throw new System.NotImplementedException();
        }
    }
}
