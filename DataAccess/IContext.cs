using System.Linq;

namespace DataAccess
{
    public interface IContext
    {
        IQueryable<T> GetAll<T>();
    }
}
