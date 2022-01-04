using Autofac;
using CQRS.Query;
using CQRS_Autofac;
using IntegrationBase;
using System.Collections.Generic;
using Xunit;

namespace CQRS_UnitTests
{
    public class TestQuery : IQuery<string> { }
    public class TestListQuery : IListQuery<string> { }

    public class TestHandler : IQueryHandler<TestQuery, string>
    {
        public string Execute(TestQuery query)
        {
            return "";
        }
    }

    public class TestListHandler : IListQueryHandler<TestListQuery, string>
    {
        public IEnumerable<string> Execute(TestListQuery query)
        {
            return new List<string>();
        }
    }
 
    public class AutofacIntegrationTest
    {
        [Fact]
        public void GivenRegistrationsOfAQuery_ItResolvesAQuery()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<IIOCResolver>().AsImplementedInterfaces();
            builder.RegisterType<QueryExecutor>().As<QueryExecutor>();
            builder.Register<TestHandler, TestQuery>();
            builder.Register<TestListHandler, TestListQuery>();
            var Container = builder.Build();

            using (var scope = Container.BeginLifetimeScope())
            {
                var executor = scope.Resolve<QueryExecutor>();

                Assert.NotNull(executor.Query(new TestQuery()));
                Assert.NotNull(executor.Query(new TestListQuery()));
            }
        }
    }
}
