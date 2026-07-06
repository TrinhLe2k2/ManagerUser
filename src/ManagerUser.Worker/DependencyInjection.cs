using ManagerUser.Application;
using ManagerUser.Infrastructure;
using ManagerUser.Worker.BackgroundServices;
using ManagerUser.Worker.BackgroundTaskQueues;
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
        // Queue phải là singleton để producer và consumer nhìn cùng một hàng đợi.
        // services.AddBackgroundTaskQueue();

        //services.AddHostedService<UserSnapshotWorker>();
        //services.AddHostedService<MultipleUserJobsWorker>();
        //services.AddHostedService<NonOverlappingWorker>();

        // Sample D cần 2 hosted service:
        // - Producer: tạo job và enqueue.
        // - Consumer: dequeue và xử lý job.
        // services.AddHostedService<BackgroundTaskQueueWorker>();
        // services.AddHostedService<BackgroundTaskQueueProducerWorker>();

        // Sample E: poll DB OutboxMessages và xử lý message pending.
        services.AddHostedService<OutboxDispatcherWorker>();

        return services;
    }
    private static IServiceCollection AddWorkerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<WorkerBusinessOptions>()
            .Bind(configuration.GetSection(WorkerBusinessOptions.SectionName))
            .Validate(options => options.BackgroundIntervalSeconds >= 5, "BackgroundIntervalSeconds must be greater than or equal to 5.")
            .Validate(options => options.PageSize is >= 1 and <= 100, "PageSize must be between 1 and 100.")
            .Validate(options => options.SampleDQueueCapacity is >= 1 and <= 1000, "SampleDQueueCapacity must be between 1 and 1000.")
            .Validate(options => options.SampleDJobDurationSeconds >= 0, "SampleDJobDurationSeconds must be greater than or equal to 0.")
            .Validate(options => options.SampleEPollIntervalSeconds >= 1, "SampleEPollIntervalSeconds must be greater than or equal to 1.")
            .Validate(options => options.SampleEBatchSize is >= 1 and <= 100, "SampleEBatchSize must be between 1 and 100.")
            .Validate(options => options.SampleELockTimeoutSeconds >= 5, "SampleELockTimeoutSeconds must be greater than or equal to 5.")
            .Validate(options => options.SampleERetryDelaySeconds >= 1, "SampleERetryDelaySeconds must be greater than or equal to 1.")
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection AddWorkerBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<UserJobBusinessService>();
        services.AddScoped<OutboxMessageProcessor>();

        return services;
    }

    private static IServiceCollection AddBackgroundTaskQueue(this IServiceCollection services)
    {
        // In-memory queue dùng Channel<T>.
        // Nếu app restart, các job chưa xử lý trong queue sẽ mất.
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

        return services;
    }
}
