using Microsoft.Extensions.DependencyInjection;
using OurHeritage.Service.Implementations;
using OurHeritage.Service.Interfaces;
using System.Reflection;

namespace OurHeritage.Service
{
    public static class ModuleServiceDependencies
    {
        public static void AddServiceDependencies(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IFollowService, FollowService>();
            services.AddMemoryCache();


            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }
    }
}
