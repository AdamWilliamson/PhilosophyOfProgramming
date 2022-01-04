using System.Collections.Generic;

namespace CQRS.Query
{
    public interface IQueryExecutor
    {
        IEnumerable<TResult> Query<TResult>(IListQuery<TResult> query);
        TResult Query<TResult>(IQuery<TResult> query);
    }
}