using FluentValidation;
using ManagerUser.Application.Common.Abstractions.Persistence;
using ManagerUser.Application.Common.Abstractions.Security;
using ManagerUser.Application.Common.Constants;
using ManagerUser.Application.Common.Exceptions;

namespace ManagerUser.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler
{
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<CreateUserCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserQueryRepository userQueryRepository,
        IUserCommandRepository userCommandRepository,
        IPasswordHasher passwordHasher,
        IValidator<CreateUserCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _userQueryRepository = userQueryRepository;
        _userCommandRepository = userCommandRepository;
        _passwordHasher = passwordHasher;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateUserCommandResult> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        if (await _userQueryRepository.ExistsByUsernameAsync(command.Username, cancellationToken))
            throw new AppException(ApplicationCodes.User.UsernameAlreadyExists, ApplicationMessages.User.UsernameAlreadyExists);

        if (await _userQueryRepository.ExistsByEmailAsync(command.Email, cancellationToken: cancellationToken))
            throw new AppException(ApplicationCodes.User.EmailAlreadyExists, ApplicationMessages.User.EmailAlreadyExists);

        var passwordHash = _passwordHasher.Hash(command.Password);
        var id = await _userCommandRepository.CreateAsync(command, passwordHash, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return new CreateUserCommandResult(id);
    }
}
