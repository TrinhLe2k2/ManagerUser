namespace ManagerUser.Application.Users.Queries.GetUserById;
public sealed record GetUserByIdQueryResult(
    Guid Id,
    string Username,
    string Email,
    string? FullName,
    byte Status,
    DateTime CreatedAt,
    DateTime? ModifiedAt);
