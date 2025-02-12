using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Repo;
using OurHeritage.Service;
using System.Text;

namespace OurHeritage.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            #region Connection String & Identity
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });



            builder.Services.AddIdentity<User, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region Register Services
            builder.Services.AddServiceDependencies();
            builder.Services.AddRepoDependencies();
            #endregion

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();


            #region Auth for swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Type = SecuritySchemeType.Http,
                    Description = "JWT Authorization header using the Bearer scheme"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });
            #endregion
            var app = builder.Build();
            #region update-Database



            // StoreContext dbContext = new StoreContext(); // Invalid
            //await dbContext.Database.MigrateAsync();
            using var Scope = app.Services.CreateScope();
            //Group of Services LifeTime Scoped
            //using is syntax suger that using to close connection instead of {Despose()}
            var Services = Scope.ServiceProvider; //Services Its Self
            var LoggerFactory = Services.GetRequiredService<ILoggerFactory>();

            try
            {

                var DbContext = Services.GetRequiredService<ApplicationDbContext>();
                // Ask CLR For Creating Object Explicitly
                await DbContext.Database.MigrateAsync();

                var UserManger = Services.GetRequiredService<UserManager<User>>();
                //await AppIdentityDbContextSeed.SeedAppUser(UserManger);
                //await StoreContextSeed.SeedAsync(DbContext);
            }
            catch (Exception ex)
            {
                var Logger = LoggerFactory.CreateLogger<Program>();
                Logger.LogError(ex, "An Error Occured During Appling The Migration");
            }
            #endregion

          
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
