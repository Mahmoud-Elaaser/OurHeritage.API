using Microsoft.Extensions.DependencyInjection;
using OurHeritage.Repo.Repositories.Implementations;
using OurHeritage.Repo.Repositories.Interfaces;

namespace OurHeritage.Repo
{
    public static class ModuleRepoDependencies
    {
        public static void AddRepoDependencies(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        }
    }
}
