using UserManagement.Application.Commands.RoleCommands.CreateRole;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("usermanagement/[controller]")]

public class CompanyRoleController : ControllerBase
{
    private readonly ISender _sender;

    public CompanyRoleController(ISender sender)
    {
        _sender = sender;
    }
    [HttpPost()]
    public async Task<ActionResult<Response<RoleViewModel>>> CreateCompanyRole([FromBody] CreateCompanyRoleCommand command)
    {
        var response = await _sender.Send(command);
        return Ok(response);
    }
}
