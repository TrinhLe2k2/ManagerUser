using FluentAssertions;
using FluentValidation;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Exceptions;
using ManagerUser.Application.Users.Queries.GetUserById;
using Moq;

namespace ManagerUser.Tests.Application.Users;

public sealed class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var expected = new GetUserByIdQueryResult(
            userId,
            "admin",
            "admin@test.com",
            "Admin",
            1,
            DateTime.UtcNow,
            null);

        var repository = new Mock<IUserQueryRepository>();
        repository
            .Setup(x => x.GetByIdAsync(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetUserByIdQueryHandler(
            repository.Object,
            new GetUserByIdQueryValidator());

        var result = await handler.HandleAsync(new GetUserByIdQuery(userId));

        result.Should().Be(expected);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowAppException_WhenUserNotFound()
    {
        var repository = new Mock<IUserQueryRepository>();
        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUserByIdQueryResult?)null);

        var handler = new GetUserByIdQueryHandler(
            repository.Object,
            new GetUserByIdQueryValidator());

        var act = () => handler.HandleAsync(new GetUserByIdQuery(Guid.NewGuid()));

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowValidationException_WhenIdIsEmpty()
    {
        var repository = new Mock<IUserQueryRepository>();

        var handler = new GetUserByIdQueryHandler(
            repository.Object,
            new GetUserByIdQueryValidator());

        var act = () => handler.HandleAsync(new GetUserByIdQuery(Guid.Empty));

        await act.Should().ThrowAsync<ValidationException>();
    }
}
