using FluentAssertions;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Users.Commands.DeleteUser;
using Moq;

namespace ManagerUser.Tests.Application.Users;

public sealed class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldDeleteUser_AndCommit_WhenUserExists()
    {
        var command = new DeleteUserCommand(Guid.NewGuid());

        var queryRepository = new Mock<IUserQueryRepository>();
        queryRepository
            .Setup(x => x.ExistsByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var commandRepository = new Mock<IUserCommandRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteUserCommandHandler(
            queryRepository.Object,
            commandRepository.Object,
            new DeleteUserCommandValidator(),
            unitOfWork.Object);

        var result = await handler.HandleAsync(command);

        result.Id.Should().Be(command.Id);
        commandRepository.Verify(x => x.DeleteAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
