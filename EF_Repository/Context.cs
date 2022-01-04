using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace POP.EF_Repository
{
    public interface IContext
    {
        IQueryable<T> All<T>() where T: class;
    }

    public class Context<TContext> : DbContext, IContext
        where TContext : DbContext
    {
        public Context(DbContextOptions<TContext> options) : base(options) {}

        public IQueryable<T> All<T>() where T : class
        {
            return this.Set<T>();
        }
    }
}
