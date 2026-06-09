using ManagerUser.Api.Constants;
using ManagerUser.Api.Mappings;
using ManagerUser.Application.Users.Commands.ChangeUserStatus;
using ManagerUser.Application.Users.Commands.CreateUser;
using ManagerUser.Application.Users.Commands.DeleteUser;
using ManagerUser.Application.Users.Commands.UpdateUser;
using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Application.Users.Queries.GetUsers;
using ManagerUser.Contracts.Common;
using ManagerUser.Contracts.Users.Requests;
using ManagerUser.Contracts.Users.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ManagerUser.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserListItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] GetUsersRequest request,
        [FromServices] GetUsersQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToQuery(), cancellationToken);

        return Ok(ApiResponse<PagedResult<UserListItemResponse>>.Ok(
            result.ToResponse(),
            ApiMessages.User.GetListSuccess,
            HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByIdAsync(
        Guid id,
        [FromServices] GetUserByIdQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id.ToGetByIdQuery(), cancellationToken);

        return Ok(ApiResponse<UserDetailResponse>.Ok(
            result.ToResponse(),
            ApiMessages.User.GetByIdSuccess,
            HttpContext.TraceIdentifier));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserCreatedResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateUserRequest request,
        [FromServices] CreateUserCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCommand(), cancellationToken);
        var response = result.ToResponse();

        Console.WriteLine("name of = " + nameof(GetByIdAsync));

        //return CreatedAtAction(
        //    nameof(GetByIdAsync),
        //    new { id = response.Id },
        //    ApiResponse<UserCreatedResponse>.Ok(
        //        response,
        //        ApiMessages.User.CreateSuccess,
        //        HttpContext.TraceIdentifier));

        return CreatedAtRoute(
            "GetUserById",
            new { id = response.Id },
            ApiResponse<UserCreatedResponse>.Ok(
                response,
                ApiMessages.User.CreateSuccess,
                HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserChangedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateUserRequest request,
        [FromServices] UpdateUserCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCommand(id), cancellationToken);

        return Ok(ApiResponse<UserChangedResponse>.Ok(
            result.ToResponse(),
            ApiMessages.User.UpdateSuccess,
            HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserChangedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        [FromServices] DeleteUserCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id.ToDeleteCommand(), cancellationToken);

        return Ok(ApiResponse<UserChangedResponse>.Ok(
            result.ToResponse(),
            ApiMessages.User.DeleteSuccess,
            HttpContext.TraceIdentifier));
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<UserChangedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatusAsync(
        Guid id,
        [FromBody] ChangeUserStatusRequest request,
        [FromServices] ChangeUserStatusCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request.ToCommand(id), cancellationToken);

        return Ok(ApiResponse<UserChangedResponse>.Ok(
            result.ToResponse(),
            ApiMessages.User.ChangeStatusSuccess,
            HttpContext.TraceIdentifier));
    }
}
