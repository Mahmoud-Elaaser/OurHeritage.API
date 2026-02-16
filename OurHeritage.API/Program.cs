using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OurHeritage.API.Hubs;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Repo;
using OurHeritage.Repo.Repositories.Implementations;
using OurHeritage.Service;
using OurHeritage.Service.Helper;

namespace OurHeritage.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<User, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            
            builder.Services.AddServiceDependencies();
            builder.Services.AddRepoDependencies();
            builder.Services.AddAuthSupport(builder.Configuration);
            builder.Services.AddCorsAndJsonSupport();

            builder.WebHost.UseWebRoot("wwwroot");


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            FilesSetting.Configure(builder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>(), builder.Configuration);


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                try
                {
                    var dbContext = services.GetRequiredService<ApplicationDbContext>();
                    await dbContext.Database.MigrateAsync(); 

                    await RoleInitializer.InitializeAsync(services); 
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occurred while applying migrations.");
                }
            }

            app.UseCors("AllowAllOrigins");
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notification");
            app.MapHub<ChatHub>("/hubs/chat");

            app.Run();
        }
    }
}
