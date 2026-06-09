using ManagerUser.Api.Mappings;
using ManagerUser.Application.Users.Queries.GetUserById;
using ManagerUser.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace ManagerUser.Api.Controllers;
public class UsersController : Controller
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByIdAsync(Guid id, [FromServices] GetUserByIdQueryHandler handler, CancellationToken cancellationToken)
    {
        id = Guid.Empty;
        var result = await handler.HandleAsync(
            new GetUserByIdQuery(id),
            cancellationToken);

        return Ok(ApiResponse<object>.Ok(result.ToResponse(), "Lay user thanh cong.", HttpContext.TraceIdentifier));
    }
}
