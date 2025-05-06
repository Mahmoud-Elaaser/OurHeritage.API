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
            services.AddTransient<IHandiCraftService, HandiCraftService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<ILikeService, LikeService>();
            services.AddTransient<ICulturalArticleService, CulturalArticleService>();
            services.AddTransient<IFavoriteService, FavoriteService>();
            services.AddTransient<IPaginationService, PaginationService>();
            services.AddTransient<IStoryService, StoryService>();
            services.AddScoped<IUserHandicraftMatchingService, UserHandicraftMatchingService>();
            services.AddScoped<IBasketService, BasketService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddScoped<IRepostService, RepostService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<INotificationService, NotificationService>();


            services.AddMemoryCache();
            services.AddSignalR();


            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }
    }
}
