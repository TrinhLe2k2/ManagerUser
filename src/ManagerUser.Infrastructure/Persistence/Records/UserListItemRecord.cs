namespace ManagerUser.Infrastructure.Persistence.Records;

public sealed class UserListItemRecord
{
    public Guid Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? FullName { get; init; }

    public byte Status { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? ModifiedAt { get; init; }

    public int TotalCount { get; init; }
}
