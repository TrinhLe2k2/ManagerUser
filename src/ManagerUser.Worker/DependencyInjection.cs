using ManagerUser.Application;
using ManagerUser.Infrastructure;
using ManagerUser.Worker.BackgroundServices;
using ManagerUser.Worker.Options;
using ManagerUser.Worker.Services;

namespace ManagerUser.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationServices();
        services.AddInfrastructureServices();

        services.AddWorkerOptions(configuration);
        services.AddWorkerBusinessServices();

        services.AddHostedService<UserSnapshotWorker>();

        return services;
    }
    private static IServiceCollection AddWorkerOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<WorkerBusinessOptions>()
            .Bind(configuration.GetSection(WorkerBusinessOptions.SectionName))
            .Validate(options => options.BackgroundIntervalSeconds >= 5, "BackgroundIntervalSeconds must be greater than or equal to 5.")
            .Validate(options => options.PageSize is >= 1 and <= 100, "PageSize must be between 1 and 100.")
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddWorkerBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<UserJobBusinessService>();

        return services;
    }
}
