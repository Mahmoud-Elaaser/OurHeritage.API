using Microsoft.EntityFrameworkCore;
using OurHeritage.Core.Context;
using OurHeritage.Repo.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OurHeritage.Repo.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Without Spec
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int Id)
        {
            return await _dbContext.Set<T>().FindAsync(Id);
        }
        #endregion

        #region With Spec
        public async Task<IReadOnlyList<T>> GetAllAsyncWithSpec(ISpecifications<T> Spec)
        {
            return await ApplySpecification(Spec).ToListAsync();
        }

        public async Task<T> GetByIdAsyncWithSpec(ISpecifications<T> Spec)
        {
            return await ApplySpecification(Spec).FirstOrDefaultAsync();
        }

        public async Task<int> GetCountWithSpecAsync(ISpecifications<T> Spec)
        {
            return await ApplySpecification(Spec).CountAsync();
        }

        /// TEntity => just to be different, not generic T
        public async Task<IEnumerable<TEntity>> FindAllAsync<TEntity>(Expression<Func<T, bool>> predicate, Expression<Func<T, TEntity>> selector)
        {
            return await _dbContext.Set<T>()
                .Where(predicate)
                .Select(selector)
                .ToListAsync();
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> match)
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(match);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().CountAsync(predicate);
        }


        public async Task<IEnumerable<T>> GetAllPredicated(Expression<Func<T, bool>> match, string[] include = null!)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (query == null) return null;

            if (include != null)
            {
                foreach (var incldes in include)
                    query = query.Include(incldes);
            }

            return await query.Where(match).ToListAsync();
        }
        public T GetEntityPredicated(Expression<Func<T, bool>> match, string[] include = null!)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (query == null)
                return null;
            if (include != null)
                foreach (var incldes in include)
                    query = query.Include(incldes);
            return query.FirstOrDefault(match);
        }


        private IQueryable<T> ApplySpecification(ISpecifications<T> Spec)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbContext.Set<T>(), Spec);
        }
        #endregion


        #region Add
        public async Task AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
        }
        public virtual async Task AddRangeAsync(ICollection<T> entities)
        {
            await _dbContext.Set<T>().AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();

        }
        #endregion

        #region Update

        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }
        public virtual async Task UpdateRangeAsync(ICollection<T> entities)
        {
            _dbContext.Set<T>().UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region Delete
        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public virtual async Task DeleteRangeAsync(ICollection<T> entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Deleted;
            }
            await _dbContext.SaveChangesAsync();
        }
        #endregion


    }
}
