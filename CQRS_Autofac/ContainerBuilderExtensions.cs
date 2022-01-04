using Autofac;
using CQRS.Query;
using System.Linq;

namespace CQRS_Autofac
{
    public static class ContainerBuilderExtensions
    {
        public static void Register<TQueryHandler, TQuery>(this ContainerBuilder builder)
        {
            var singleQueryType = typeof(TQueryHandler).GetInterface(typeof(IQueryHandler<,>).Name);
            var listQueryType = typeof(TQueryHandler).GetInterface(typeof(IListQueryHandler<,>).Name);

            if (listQueryType != null)
            {
                var genericArgs = listQueryType.GetGenericArguments();
                var queryType = genericArgs.First();
                var returnType = genericArgs.Last();
                builder.RegisterType<TQueryHandler>().As(typeof(IListQueryHandler<,>).MakeGenericType(queryType, returnType));
            }
            else if (singleQueryType != null)
            {
                var genericArgs = singleQueryType.GetGenericArguments();
                var queryType = genericArgs.First();
                var returnType = genericArgs.Last();
                builder.RegisterType<TQueryHandler>().As(typeof(IQueryHandler<,>).MakeGenericType(queryType, returnType));
            }
        }
    }
}
