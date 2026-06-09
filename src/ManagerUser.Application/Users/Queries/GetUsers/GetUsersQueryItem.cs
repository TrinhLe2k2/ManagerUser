namespace ManagerUser.Application.Users.Queries.GetUsers;
public sealed record GetUsersQueryItem(
    Guid Id,
    string Username,
    string Email,
    string? FullName,
    byte Status,
    DateTime CreatedAt,
    DateTime? ModifiedAt);
