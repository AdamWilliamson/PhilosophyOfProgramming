using System;
using System.Linq.Expressions;

public class GenericSpecification<TEntity> : SpecificationBase<TEntity>
{
    protected Expression<Func<TEntity, bool>> _predicate;
    
    public GenericSpecification(Expression<Func<TEntity, bool>> predicate)
    {
        _predicate = predicate;
    }

    public override Expression<Func<TEntity, bool>> GetPredicate() { return _predicate; }
}
