using FluentAssertions;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Exceptions;
using ManagerUser.Application.Users.Commands.UpdateUser;
using Moq;

namespace ManagerUser.Tests.Application.Users;

public sealed class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldUpdateUser_AndCommit_WhenUserExists()
    {
        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            "new@test.com",
            "New Name");

        var queryRepository = new Mock<IUserQueryRepository>();
        queryRepository
            .Setup(x => x.ExistsByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        queryRepository
            .Setup(x => x.ExistsByEmailAsync(command.Email, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var commandRepository = new Mock<IUserCommandRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateUserCommandHandler(
            queryRepository.Object,
            commandRepository.Object,
            new UpdateUserCommandValidator(),
            unitOfWork.Object);

        var result = await handler.HandleAsync(command);

        result.Id.Should().Be(command.Id);
        commandRepository.Verify(x => x.UpdateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowAppException_WhenUserNotFound()
    {
        var command = new UpdateUserCommand(
            Guid.NewGuid(),
            "new@test.com",
            "New Name");

        var queryRepository = new Mock<IUserQueryRepository>();
        queryRepository
            .Setup(x => x.ExistsByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UpdateUserCommandHandler(
            queryRepository.Object,
            Mock.Of<IUserCommandRepository>(),
            new UpdateUserCommandValidator(),
            Mock.Of<IUnitOfWork>());

        var act = () => handler.HandleAsync(command);

        await act.Should().ThrowAsync<AppException>();
    }
}
