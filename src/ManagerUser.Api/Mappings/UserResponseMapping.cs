using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Contracts.Users.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ManagerUser.Api.Mappings;

public static class UserResponseMapping
{
    public static UserDetailResponse ToResponse(this GetUserByIdQueryResult result)
    {
        return new UserDetailResponse
        {
            Id = result.Id,
            Email = result.Email,
            FullName = result.FullName ?? string.Empty,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.ModifiedAt
        };
    }
}
