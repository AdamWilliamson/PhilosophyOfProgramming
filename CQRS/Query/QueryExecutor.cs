using IntegrationBase;
using System.Collections.Generic;

namespace CQRS.Query
{
    public class QueryExecutor : IQueryExecutor
    {
        IIOCResolver container;

        public QueryExecutor(IIOCResolver container)
        {
            this.container = container;
        }

        public TResult Query<TResult>(IQuery<TResult> query)
        {
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var handler = (dynamic)container.Resolve(handlerType);
            return handler.Execute((dynamic)query);
        }

        public IEnumerable<TResult> Query<TResult>(IListQuery<TResult> query)
        {
            var handlerType = typeof(IListQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var handler = (dynamic)container.Resolve(handlerType);
            return handler.Execute((dynamic)query);
        }
    }
}
