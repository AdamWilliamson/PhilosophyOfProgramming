using System.Collections.Generic;

namespace Repository
{
    public interface IGetRepository<TEntity, TKey>
        where TEntity : class
    {
        TEntity Get(TKey id);
    }

    public interface IListRepository<TEntity, TKey>
        where TEntity : class
    {
        IEnumerable<TEntity> List();
    }

    public interface IGetSingleRepository<TEntity, TKey>
        where TEntity : class
    {
        TEntity Single(ISpecification<TEntity> specification);
        TEntity Single(TKey id);
    }

    public interface ISpecificationFilterRepository<TEntity, TKey>
        where TEntity : class
    {
        IEnumerable<TEntity> Filter(ISpecification<TEntity> specification);
    }

    public interface IAddItemRepository<TEntity, TKey>
        where TEntity : class
    {
        void Add(TEntity entity);
        void Add(IEnumerable<TEntity> entities);
    }

    public interface IDeleteItemRepository<TEntity, TKey>
        where TEntity : class
    {
        void Delete(TKey id);
        void Delete(TEntity entity);
        void Delete(IEnumerable<TEntity> entities);
    }

    public interface IReadOnlyRepository<TEntity, TKey>
        : IGetRepository<TEntity, TKey>,
        IListRepository<TEntity, TKey>,
        IGetSingleRepository<TEntity, TKey>,
        ISpecificationFilterRepository<TEntity, TKey>
        where TEntity : class
    {  }

    public interface IRepository<TEntity, TKey>
        : IReadOnlyRepository<TEntity, TKey>,
        IAddItemRepository<TEntity, TKey>,
        IDeleteItemRepository<TEntity, TKey>
        where TEntity : class
    { }
}
