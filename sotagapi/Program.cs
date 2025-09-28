using Microsoft.EntityFrameworkCore;
using sotagapi.DbContexts;
using sotagapi.Services;

namespace sotagapi
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            SQLitePCL.Batteries_V2.Init();
#endif
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:4200",
                            "https://localhost:4200"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddScoped<TagService>();
            builder.Services.AddHttpClient<StackOverflowClient>();

            builder.Services.AddControllers();
            
            builder.Services.AddDbContext<TagsDbContext>(options =>
                options.UseSqlite("Data Source=/app/tags.db"));


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TagsDbContext>();
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            app.UseCors("AllowAngular");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
