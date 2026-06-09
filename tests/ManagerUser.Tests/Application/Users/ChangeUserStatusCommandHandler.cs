using FluentAssertions;
using FluentValidation;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Users.Commands.ChangeUserStatus;
using Moq;

namespace ManagerUser.Tests.Application.Users;

public sealed class ChangeUserStatusCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldChangeStatus_AndCommit_WhenUserExists()
    {
        var command = new ChangeUserStatusCommand(Guid.NewGuid(), 1);

        var queryRepository = new Mock<IUserQueryRepository>();
        queryRepository
            .Setup(x => x.ExistsByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var commandRepository = new Mock<IUserCommandRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new ChangeUserStatusCommandHandler(
            queryRepository.Object,
            commandRepository.Object,
            new ChangeUserStatusCommandValidator(),
            unitOfWork.Object);

        var result = await handler.HandleAsync(command);

        result.Id.Should().Be(command.Id);
        commandRepository.Verify(x => x.ChangeStatusAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowValidationException_WhenStatusIsInvalid()
    {
        var command = new ChangeUserStatusCommand(Guid.NewGuid(), 9);

        var handler = new ChangeUserStatusCommandHandler(
            Mock.Of<IUserQueryRepository>(),
            Mock.Of<IUserCommandRepository>(),
            new ChangeUserStatusCommandValidator(),
            Mock.Of<IUnitOfWork>());

        var act = () => handler.HandleAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
