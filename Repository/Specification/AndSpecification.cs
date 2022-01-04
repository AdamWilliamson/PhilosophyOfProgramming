using System;
using System.Linq;
using System.Linq.Expressions;

public class AndSpecification<TEntity> : SpecificationBase<TEntity>
{
    protected Expression<Func<TEntity, bool>> _predicate;

    public AndSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
    {
        var andExpression = Expression.AndAlso(left.GetPredicate().Body, right.GetPredicate().Body);

        _predicate = Expression.Lambda<Func<TEntity, bool>>(andExpression, left.GetPredicate().Parameters.Single());
    }

    public override Expression<Func<TEntity, bool>> GetPredicate() { return _predicate; }
}
