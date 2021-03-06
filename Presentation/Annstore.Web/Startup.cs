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
using Annstore.Query;
using Annstore.Core.Events;
using Annstore.Services.Events;
using Annstore.Core.Common;
using Annstore.Data.Catalog;
using Annstore.Data.Customers;
using Microsoft.Extensions.Options;
using Annstore.Query.Infrastructure;
using Annstore.DataMixture.Events;
using Annstore.DataMixture.Services.Catalog;
using Annstore.DataMixture;
using Annstore.DataMixture.Mappings;
using Annstore.DataMixture.DataMixtures;
using Annstore.Framework.Events;

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
            //db contexts
            var migrationAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            services.AddDbContext<AnnstoreDbContext>(
                opts => opts.UseSqlServer(_configuration.GetConnectionString(Defaults.ConnectionStrings.Data),
                sqlOpts => sqlOpts.MigrationsAssembly(migrationAssemblyName)));
            services.AddDbContext<AnnstoreAuthDbContext>(
                opts => opts.UseSqlServer(_configuration.GetConnectionString(Defaults.ConnectionStrings.Auth),
                sqlOpts => sqlOpts.MigrationsAssembly(migrationAssemblyName)));

            //query
            services.AddSingleton<IQueryDbSettings>(sp =>
                sp.GetRequiredService<IOptions<QueryDbSettings>>().Value);
            services.AddScoped<IQueryDbContext, AnnstoreQueryDbContext>();
            services.AddTransient(typeof(IQueryRepository<>), typeof(QueryRepository<>));

            //mix
            services.AddScoped(typeof(IMixRepository<>), typeof(MixRepository<>));
            services.AddTransient<IMixCategoryService, MixCategoryService>();
            services.AddScoped<ICategoryDataMixturer, CategoryDataMixturer>();
            services.AddScoped<ICategoryDataDependencyResolver, CategoryDataDependencyResolver>();

            //identity
            services.AddIdentity<Account, Role>()
                .AddEntityFrameworkStores<AnnstoreAuthDbContext>()
                .AddDefaultTokenProviders();

            //services
            services.AddScoped<IDbContext, AnnstoreDbContext>();
            services.AddScoped<IQueryDbContext, AnnstoreQueryDbContext>();
            services.AddTransient(typeof(IRepository<>), typeof(RepositoryBase<>));
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<ICustomerRepository, CustomerRepository>();
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
                mc.AddProfile<MixDataProfile>();
            });
            var mapper = mapperConfiguration.CreateMapper();
            services.AddSingleton(mapper);

            //mvc
            services.AddMvc()
                .AddFluentValidation(opts =>
                {
                    opts.RegisterValidatorsFromAssembly(typeof(Startup).Assembly);
                    opts.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                });

            //helpers
            var assemblyHelper = new AssemblyHelper();
            services.AddSingleton<IAssemblyHelper>(assemblyHelper);
            services.AddSingleton<IStringHelper, StringHelper>();

            //event
            services.AddScoped<IEventPublisher, EventPublisher>();
            services.AddScoped<DefaultEventHandler>();
            services.AddScoped<DomainCategoryEventHandler>();
            services.AddScoped<MixCategoryEventHandler>();
            EventPublisher.RegisterEventHandlers(assemblyHelper.GetAppOwnAssemblies());

            //settings
            var categorySettingsSection = _configuration.GetSection("CategorySettings");
            services.Configure<CategorySettings>(categorySettingsSection);

            var userSettingsSection = _configuration.GetSection("UserSettings");
            services.Configure<AccountSettings>(userSettingsSection);

            var customerSettingsSection = _configuration.GetSection("CustomerSettings");
            services.Configure<CustomerSettings>(customerSettingsSection);

            var queryDbSettingsSection = _configuration.GetSection("QueryDbSettings");
            services.Configure<QueryDbSettings>(queryDbSettingsSection);

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
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
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
