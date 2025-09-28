using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sotagapi.DbContexts;
using sotagapi.Models;
using System.Linq;

namespace sotagapi.Tests.Integration
{
    public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {
        private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextServices = services.Where(d =>
                    d.ServiceType == typeof(TagsDbContext) ||
                    d.ServiceType == typeof(DbContextOptions<TagsDbContext>) ||
                    d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextFactory<>) ||
                    d.ImplementationFactory?.Target.GetType().FullName?.Contains("TagsDbContext") == true ||
                    d.ImplementationInstance?.GetType().FullName?.Contains("TagsDbContext") == true ||
                    d.ImplementationType?.FullName?.Contains("TagsDbContext") == true)
                    .ToList();

                var applicationDbContextServices = services.Where(s =>
                    s.ServiceType.FullName?.Contains("TagsDbContext") == true).ToList();

                var servicesToRemove = dbContextServices.Union(applicationDbContextServices).Distinct().ToList();

                foreach (var service in servicesToRemove)
                {
                    services.Remove(service);
                }

                services.AddDbContext<TagsDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<TagsDbContext>();

                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    SeedData(db);
                }
            });
        }

        private void SeedData(TagsDbContext context)
        {
            context.Tags.AddRange(new[]
            {
            new Tag { Id = 1, Name = "csharp", Count = 100, FetchedAt = DateTime.UtcNow.AddHours(-1) },
            new Tag { Id = 2, Name = "java", Count = 50, FetchedAt = DateTime.UtcNow.AddHours(-2) },
            new Tag { Id = 3, Name = "python", Count = 40, FetchedAt = DateTime.UtcNow.AddHours(-3) }
        });
            context.SaveChanges();
        }
    }
}
