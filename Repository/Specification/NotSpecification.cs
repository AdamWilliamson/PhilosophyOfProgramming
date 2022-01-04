using System;
using System.Linq.Expressions;

public class NotSpecification<TEntity> : SpecificationBase<TEntity>
{
    protected Expression<Func<TEntity, bool>> _predicate;

    public NotSpecification(ISpecification<TEntity> specification)
    {
        _predicate = Expression.Lambda<Func<TEntity, bool>>(Expression.Not(specification.GetPredicate()));
    }

    public override Expression<Func<TEntity, bool>> GetPredicate() { return _predicate; }
}
