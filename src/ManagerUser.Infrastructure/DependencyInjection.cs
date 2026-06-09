using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Infrastructure.Persistence.Connections;
using ManagerUser.Infrastructure.Persistence.Repositories;
using ManagerUser.Infrastructure.Persistence.Sessions;
using ManagerUser.Infrastructure.Persistence.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace ManagerUser.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<SqlConnectionFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<DbSession>();
        return services;
    }
}
