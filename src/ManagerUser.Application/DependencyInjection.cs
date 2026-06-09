using FluentValidation;
using ManagerUser.Application.Users.Queries.GetUserById;
using Microsoft.Extensions.DependencyInjection;

namespace ManagerUser.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<GetUserByIdQueryHandler>();

        services.AddScoped<IValidator<GetUserByIdQuery>, GetUserByIdQueryValidator>();

        return services;
    }
}
