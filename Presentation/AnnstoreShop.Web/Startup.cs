using System;
using Annstore.Data;
using Annstore.Services.Catalog;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentValidation.AspNetCore;
using Annstore.Web.Areas.Admin.Mappings;
using Annstore.Web.Infrastructure;
using Annstore.Core.Entities.Users;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using Annstore.Web.Areas.Admin.Services.Categories;
using Annstore.Web.Areas.Admin.Services.Users;
using Annstore.Web.Services.Accounts;

namespace Annstore.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AnnstoreDbContext>(
                opts => opts.UseSqlServer(_configuration.GetConnectionString(Defaults.ConnectionStringName),
                sqlOpts => sqlOpts.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name)));
            services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<AnnstoreDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<IDbContext, AnnstoreDbContext>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IAdminCategoryService, AdminCategoryService>();
            services.AddTransient<IAdminUserService, AdminUserService>();
            services.AddTransient<IAccountService, AccountService>();

            //auto mapper
            var mapperConfiguration = new MapperConfiguration(mc =>
            {
                mc.AddProfile<AdminModelProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();
            services.AddSingleton(mapper);

            services.AddMvc()
                .AddFluentValidation(opts =>
                {
                    opts.RegisterValidatorsFromAssembly(typeof(Startup).Assembly);
                    opts.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                });

            //settings
            var categorySettingsSection = _configuration.GetSection("CategorySettings");
            services.Configure<CategorySettings>(categorySettingsSection);

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }
            else
            {
                app.UseExceptionHandler("/Errors");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "area_default",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
