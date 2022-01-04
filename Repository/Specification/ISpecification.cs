using System;
using System.Linq.Expressions;

public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>> GetPredicate();

    bool IsSatisfiedBy(TEntity item);
}
