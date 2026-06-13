using FluentAssertions;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Abstractions.Security;
using ManagerUser.Application.Common.Exceptions;
using ManagerUser.Application.Users.Commands.CreateUser;
using Moq;

namespace ManagerUser.Tests.Application.Users;

public sealed class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldCreateUser_AndCommit_WhenInputIsValid()
    {
        var command = new CreateUserCommand(
            "admin",
            "admin@test.com",
            "Admin",
            "Password123",
            1);

        var createdId = Guid.NewGuid();

        var queryRepository = new Mock<IUserQueryRepository>();
        queryRepository
            .Setup(x => x.ExistsByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        queryRepository
            .Setup(x => x.ExistsByEmailAsync(command.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var commandRepository = new Mock<IUserCommandRepository>();
        commandRepository
            .Setup(x => x.CreateAsync(command, "HASHED_PASSWORD", It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdId);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.Hash(command.Password))
            .Returns("HASHED_PASSWORD");

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateUserCommandHandler(
            queryRepository.Object,
            commandRepository.Object,
            passwordHasher.Object,
            new CreateUserCommandValidator(),
            unitOfWork.Object);

        var result = await handler.HandleAsync(command);

        result.Id.Should().Be(createdId);
        passwordHasher.Verify(x => x.Hash(command.Password), Times.Once);
        commandRepository.Verify(x => x.CreateAsync(command, "HASHED_PASSWORD", It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowAppException_WhenUsernameAlreadyExists()
    {
        var command = new CreateUserCommand(
            "admin",
            "admin@test.com",
            "Admin",
            "Password123",
            1);

        var queryRepository = new Mock<IUserQueryRepository>();
        queryRepository
            .Setup(x => x.ExistsByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateUserCommandHandler(
            queryRepository.Object,
            Mock.Of<IUserCommandRepository>(),
            Mock.Of<IPasswordHasher>(),
            new CreateUserCommandValidator(),
            Mock.Of<IUnitOfWork>());

        var act = () => handler.HandleAsync(command);

        await act.Should().ThrowAsync<AppException>();
    }
}
