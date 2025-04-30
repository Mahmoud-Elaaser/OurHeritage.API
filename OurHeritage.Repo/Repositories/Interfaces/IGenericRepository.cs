using OurHeritage.Core.Specifications;
using System.Linq.Expressions;

namespace OurHeritage.Repo.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>> GetAllPredicated(Expression<Func<T, bool>> predicate, string[] includes);

    }
}
