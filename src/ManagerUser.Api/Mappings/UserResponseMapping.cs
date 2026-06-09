using ManagerUser.Application.Users.Commands.ChangeUserStatus;
using ManagerUser.Application.Users.Commands.CreateUser;
using ManagerUser.Application.Users.Commands.DeleteUser;
using ManagerUser.Application.Users.Commands.UpdateUser;
using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Application.Users.Queries.GetUsers;
using ManagerUser.Contracts.Common;
using ManagerUser.Contracts.Users.Responses;

namespace ManagerUser.Api.Mappings;

public static class UserResponseMapping
{
    public static UserDetailResponse ToResponse(this GetUserByIdQueryResult result)
    {
        return new UserDetailResponse
        {
            Id = result.Id,
            Username = result.Username,
            Email = result.Email,
            FullName = result.FullName ?? string.Empty,
            Status = result.Status,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.ModifiedAt
        };
    }

    public static PagedResult<UserListItemResponse> ToResponse(this GetUsersQueryResult result)
    {
        return new PagedResult<UserListItemResponse>
        {
            Items = result.Items.Select(item => item.ToResponse()).ToList(),
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public static UserListItemResponse ToResponse(this GetUsersQueryItem item)
    {
        return new UserListItemResponse
        {
            Id = item.Id,
            Username = item.Username,
            Email = item.Email,
            FullName = item.FullName ?? string.Empty,
            Status = item.Status,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.ModifiedAt
        };
    }

    public static UserCreatedResponse ToResponse(this CreateUserCommandResult result)
    {
        return new UserCreatedResponse
        {
            Id = result.Id
        };
    }

    public static UserChangedResponse ToResponse(this UpdateUserCommandResult result)
    {
        return new UserChangedResponse
        {
            Id = result.Id
        };
    }

    public static UserChangedResponse ToResponse(this DeleteUserCommandResult result)
    {
        return new UserChangedResponse
        {
            Id = result.Id
        };
    }

    public static UserChangedResponse ToResponse(this ChangeUserStatusCommandResult result)
    {
        return new UserChangedResponse
        {
            Id = result.Id
        };
    }
}
