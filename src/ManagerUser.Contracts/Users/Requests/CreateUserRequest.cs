namespace ManagerUser.Contracts.Users.Requests;

public sealed class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string Password { get; set; } = string.Empty;

    public byte Status { get; set; } = 1;
}
