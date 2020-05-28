using Annstore.Auth;
using Annstore.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Annstore.Web.IntegratedTests
{
    public class TestAnnstoreWebApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AnnstoreDbContext));
                var authDbContextDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AnnstoreAuthDbContext));
                if(dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }
                if(authDbContextDescriptor != null)
                {
                    services.Remove(authDbContextDescriptor);
                }
                services.AddDbContext<AnnstoreDbContext>(opts =>
                {
                    opts.UseInMemoryDatabase("TestAnnstoreDb");
                });
                services.AddDbContext<AnnstoreAuthDbContext>(opts =>
                {
                    opts.UseInMemoryDatabase("TestAnnstoreAuthDb");
                });

                var sp = services.BuildServiceProvider();

                using(var scope = sp.CreateScope())
                {
                    var scopeServices = scope.ServiceProvider;
                    var db = scopeServices.GetRequiredService<AnnstoreDbContext>();
                    var authDb = scopeServices.GetRequiredService<AnnstoreAuthDbContext>();

                    db.Database.EnsureCreated();
                    authDb.Database.EnsureCreated();
                }
            });
        }
    }
}
