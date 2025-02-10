using OurHeritage.Core.Context;
using OurHeritage.Repo.Repositories.Interfaces;
using System.Collections;

namespace OurHeritage.Repo.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private Hashtable _repositories;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Hashtable();
        }
        public async Task<int> Complete()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return _repositories[typeof(T)] as IGenericRepository<T>;
            }

            var repository = new GenericRepository<T>(_dbContext);
            _repositories[typeof(T)] = repository;
            return repository;
        }

        public IGenericRepository<T> Repository2<T>() where T : class
        {
            var Type = typeof(T).Name;
            if (!_repositories.ContainsKey(Type))
            {
                var Repository = new GenericRepository<T>(_dbContext);
                _repositories.Add(Type, Repository);
            }
            return _repositories[Type] as IGenericRepository<T>;
        }
    }
}
