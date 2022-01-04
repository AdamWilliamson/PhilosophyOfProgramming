using System.Collections.Generic;

namespace CQRS.Query
{
    public interface IQuery<out TResult> { }
    public interface IListQuery<out TResult> { }

    public interface IQueryHandler<in TQuery, out TResult> : IQuery<TResult>
        where TQuery : IQuery<TResult>
    {
        TResult Execute(TQuery query);
    }

    public interface IListQueryHandler<in TQuery, out TResult> : IListQuery<TResult>
        where TQuery : IListQuery<TResult>
    {
        IEnumerable<TResult> Execute(TQuery query);
    }
}
