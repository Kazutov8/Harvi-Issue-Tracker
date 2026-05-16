using System.Collections.Generic;
using IssueTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IssueTracker.API.IntegrationTests.TestHost;

public sealed class ApiTestFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"issue-tracker-tests-{Guid.NewGuid():N}.db");
    private readonly Action<IServiceCollection>? _configureServices;
    private readonly Func<IServiceProvider, Task>? _seedDatabaseAsync;

    public ApiTestFactory(Action<IServiceCollection>? configureServices = null, Func<IServiceProvider, Task>? seedDatabaseAsync = null)
    {
        _configureServices = configureServices;
        _seedDatabaseAsync = seedDatabaseAsync;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                ["Jwt:Issuer"] = "IssueTracker",
                ["Jwt:Audience"] = "IssueTracker.Frontend",
                ["Jwt:SigningKey"] = "development-signing-key-change-before-production",
                ["Jwt:ExpirationMinutes"] = "480",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<IssueTrackerDbContext>>();
            services.RemoveAll<IssueTrackerDbContext>();

            services.AddDbContext<IssueTrackerDbContext>(options =>
                options.UseSqlite($"Data Source={_databasePath}"));

            _configureServices?.Invoke(services);

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IssueTrackerDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();
            _seedDatabaseAsync?.Invoke(scope.ServiceProvider).GetAwaiter().GetResult();
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        try
        {
            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
        catch
        {
            // Ignore cleanup failures for temporary test databases.
        }
    }
}
