namespace UserManagement.Api.Controllers;

[ApiController]
[Route("UserManagement/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ISender _sender;
    public CompanyController(ISender sender)
    {
        _sender = sender;
    }
    [HttpPost]
    public async Task<ActionResult<Response<CompanyCreateViewModel>>> CreateCompany([FromBody] CreateCompanyCommand request)
    {
        return await _sender.Send(request);
    }
}
