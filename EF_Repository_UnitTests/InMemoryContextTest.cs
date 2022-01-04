using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace EF_Repository_UnitTests
{
    public class InMemoryContextTest
    {
        [Fact]
        public void Test1()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "UnitTestDB")
                .Options;

            // Insert seed data into the database using one instance of the context
            using (var context = new TestContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.UnitTestEntity.Add(new UnitTestEntity());
                context.SaveChanges();
            }

            // Use a clean instance of the context to run the test
            using (var context = new TestContext(options))
            {
                var result = context.All<UnitTestEntity>();

                result.ToList().Count().Should().Be(0);
            }
        }
    }
}
