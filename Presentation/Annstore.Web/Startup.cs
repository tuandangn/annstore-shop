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
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using Annstore.Application.Services.Categories;
using Annstore.Auth;
using Annstore.Auth.Entities;
using Annstore.Application.Services.Customers;
using Annstore.Application.Mappings;
using Annstore.Services.Customers;
using Annstore.Application.Infrastructure.Settings;
using Annstore.Auth.Services;

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
            var migrationAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            services.AddDbContext<AnnstoreDbContext>(
                opts => opts.UseSqlServer(_configuration.GetConnectionString(Defaults.ConnectionStrings.Data),
                sqlOpts => sqlOpts.MigrationsAssembly(migrationAssemblyName)));
            services.AddDbContext<AnnstoreAuthDbContext>(
                opts => opts.UseSqlServer(_configuration.GetConnectionString(Defaults.ConnectionStrings.Auth),
                sqlOpts => sqlOpts.MigrationsAssembly(migrationAssemblyName)));

            services.AddIdentity<Account, Role>()
                .AddEntityFrameworkStores<AnnstoreAuthDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IDbContext, AnnstoreDbContext>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IAdminCategoryService, AdminCategoryService>();
            services.AddTransient<IAdminCustomerService, AdminCustomerService>();
            services.AddTransient<IAdminAccountService, AdminAccountService>();
            services.AddTransient<IPublicAccountService, PublicAccountService>();
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

            var userSettingsSection = _configuration.GetSection("UserSettings");
            services.Configure<AccountSettings>(userSettingsSection);

            var customerSettingsSection = _configuration.GetSection("CustomerSettings");
            services.Configure<CustomerSettings>(customerSettingsSection);

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

                options.LoginPath = "/Account/SignIn";
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
