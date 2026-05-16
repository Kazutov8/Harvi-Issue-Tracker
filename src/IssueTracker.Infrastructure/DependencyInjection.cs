using System.Text;
using IssueTracker.Application.Abstractions;
using IssueTracker.Application.Auth;
using IssueTracker.Application.Issues;
using IssueTracker.Application.Labels;
using IssueTracker.Application.Projects;
using IssueTracker.Infrastructure.AI;
using IssueTracker.Infrastructure.Auth;
using IssueTracker.Infrastructure.Persistence;
using IssueTracker.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IssueTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration section was not found.");

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Issuer)
                && !string.IsNullOrWhiteSpace(options.Audience)
                && !string.IsNullOrWhiteSpace(options.SigningKey),
                "JWT configuration is invalid.")
            .ValidateOnStart();

        services.AddOptions<TriageOptions>()
            .Bind(configuration.GetSection(TriageOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.BaseUrl)
                && !string.IsNullOrWhiteSpace(options.Model)
                && options.TimeoutSeconds > 0,
                "Triage AI configuration is invalid.")
            .ValidateOnStart();

        services.AddDbContext<IssueTrackerDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddHttpClient<ITriageAgent, LlmTriageAgent>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TriageOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.BaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<IssueTrackerDbContext>());
        services.AddScoped<IIssueRepository, IssueRepository>();
        services.AddScoped<ILabelRepository, LabelRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<CreateIssue>();
        services.AddScoped<CreateProject>();
        services.AddScoped<GetIssueDetails>();
        services.AddScoped<ListProjects>();
        services.AddScoped<ListProjectIssues>();
        services.AddScoped<ListProjectLabels>();
        services.AddScoped<SuggestIssueTriage>();
        services.AddScoped<RegisterUser>();
        services.AddScoped<LoginUser>();
        services.AddScoped<GetCurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        return services;
    }
}
