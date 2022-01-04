using Microsoft.EntityFrameworkCore;
using NSubstitute;
using POP.EF_Repository;

namespace EF_Repository_UnitTests
{
    class TestContext : Context<TestContext>
    {
        public TestContext(DbContextOptions<TestContext> options) : base(options)
        {
            UnitTestEntity = Substitute.For<DbSet<UnitTestEntity>>();
        }

        public DbSet<UnitTestEntity> UnitTestEntity { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UnitTestEntity>(
                b =>
                {
                    b.Property("Id");
                    b.HasKey("Id");
                });
        }
    }
}
