using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annstore.Data;
using Annstore.Services;
using Annstore.Services.Catalog;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Annstore.Web.Areas.Admin.Mappings;

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
            services.AddDbContext<AnnstoreDbContext>(opts => opts.UseSqlServer(_configuration.GetConnectionString("AnnstoreDb"), sqlOpts => sqlOpts.MigrationsAssembly("AnnstoreShop.Web")));
            services.AddScoped<IDbContext, AnnstoreDbContext>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<ICategoryService, CategoryService>();

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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "area_default",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
