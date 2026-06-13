namespace ManagerUser.Contracts.Users.Requests;

public sealed class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;

    public string? FullName { get; set; }
}
