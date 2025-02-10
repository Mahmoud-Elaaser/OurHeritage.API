using System.Linq.Expressions;

namespace OurHeritage.Repo.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);

        Task<IReadOnlyList<T>> GetAllAsyncWithSpec(ISpecifications<T> Spec);
        Task<T> GetByIdAsyncWithSpec(ISpecifications<T> Spec);
        Task<int> GetCountWithSpecAsync(ISpecifications<T> Spec);
        Task<T> FindAsync(Expression<Func<T, bool>> match);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(Expression<Func<T, bool>> predicate, Expression<Func<T, TEntity>> selector);
        Task<IEnumerable<T>> GetAllPredicated(Expression<Func<T, bool>> match, string[] include = null!);
        T GetEntityPredicated(Expression<Func<T, bool>> match, string[] include = null!);

        Task AddAsync(T entity);
        void Delete(T entity);
        void Update(T entity);

        //---------------------------------------------
        Task DeleteRangeAsync(ICollection<T> entities);
        Task AddRangeAsync(ICollection<T> entities);
        Task UpdateRangeAsync(ICollection<T> entities);


    }
}
