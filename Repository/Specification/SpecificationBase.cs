using System;
using System.Linq.Expressions;

public abstract class SpecificationBase<TEntity> : ISpecification<TEntity>
{
    public abstract Expression<Func<TEntity, bool>> GetPredicate();

    public bool IsSatisfiedBy(TEntity entity)
    {
        return GetPredicate().Compile().Invoke(entity);
    }
}
