using MediatR;
using Shared.Domain.DTOs;
using UserManagement.Application.Commands.UserCommands.CreateUser;
using UserManagement.Application.ViewModels;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("security/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }



    [HttpPost]
    public async Task<ActionResult<Response<UserViewModel>>> save([FromBody] CreateUserCommand request)
    {
        return await _sender.Send(request);
    }


}
