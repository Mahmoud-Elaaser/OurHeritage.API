using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OurHeritage.Core.Context;
using OurHeritage.Core.Entities;
using OurHeritage.Repo;
using OurHeritage.Repo.Repositories.Implementations;
using OurHeritage.Service;
using OurHeritage.Service.DTOs;
using OurHeritage.Service.Helper;
using Stripe;
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

            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>();
            StripeConfiguration.ApiKey = stripeSettings.SecretKey;

            #endregion

            #region Register Services
            builder.Services.AddServiceDependencies();
            builder.Services.AddRepoDependencies();
            #endregion
            builder.WebHost.UseWebRoot("wwwroot");

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                });


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            FilesSetting.Configure(builder.Services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>(), builder.Configuration);





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
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            var app = builder.Build();


            #region Database Initialization & Role Seeding

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                try
                {
                    var dbContext = services.GetRequiredService<ApplicationDbContext>();
                    await dbContext.Database.MigrateAsync(); // Apply pending migrations

                    await RoleInitializer.InitializeAsync(services); // Seed roles if not exists
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<Program>();
                    logger.LogError(ex, "An error occurred while applying migrations.");
                }
            }
            #endregion
            app.UseCors("AllowAllOrigins");
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
